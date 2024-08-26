using Discord;
using Discord.WebSocket;
using Nino.Utilities;
using Nino.Records;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Localizer.Localizer;
using Microsoft.Azure.Cosmos;

namespace Nino.Commands
{
    internal static class NewProject
    {
        public const string Name = "newproject";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);
            var member = guild.GetUser(interaction.User.Id);
            var lng = interaction.UserLocale;
            if (!member.GuildPermissions.Administrator) return await Response.Fail(T("error.notPrivileged", lng), interaction);

            log.Info("Beginning project creation");

            // Get inputs
            var nickname = ((string)interaction.Data.Options.FirstOrDefault(o => o.Name == "nickname")!.Value).Trim();
            var title = ((string)interaction.Data.Options.FirstOrDefault(o => o.Name == "title")!.Value).Trim();
            var type = (ProjectType)Convert.ToInt32(interaction.Data.Options.FirstOrDefault(o => o.Name == "projecttype")!.Value);
            var length = Convert.ToInt32(interaction.Data.Options.FirstOrDefault(o => o.Name == "length")!.Value);
            var posterUri = ((string)interaction.Data.Options.FirstOrDefault(o => o.Name == "poster")!.Value).Trim();
            var isPrivate = (bool)interaction.Data.Options.FirstOrDefault(o => o.Name == "private")!.Value;
            var updateChannelId = ((SocketChannel)interaction.Data.Options.FirstOrDefault(o => o.Name == "updatechannel")!.Value).Id;
            var releaseChannelId = ((SocketChannel)interaction.Data.Options.FirstOrDefault(o => o.Name == "releasechannel")!.Value).Id;
            var ownerId = interaction.User.Id;

            // Verify data
            // TODO: if name exists, reject

            // Populate data

            var projectData = new Project
            {
                Id = $"{guildId}-{nickname}",
                GuildId = guildId,
                Nickname = nickname,
                Title = title,
                OwnerId = ownerId,
                Type = type,
                PosterUri = posterUri,
                UpdateChannelId = updateChannelId,
                ReleaseChannelId = releaseChannelId,
                IsPrivate = isPrivate,
                AdministratorIds = [],
                KeyStaff = [],
                CongaParticipants = [],
                Aliases = [],
            };

            var episodes = new List<Episode>();
            for (var i = 1; i <= length; i++)
            {
                episodes.Add(new Episode
                {
                    Id = $"{projectData.Id}-{i}",
                    ProjectId = projectData.Id,
                    Number = i,
                    Done = false,
                    ReminderPosted = false,
                    AdditionalStaff = [],
                    Tasks = [],
                });
            }

            // Add to database
            await AzureHelper.Projects!.UpsertItemAsync(projectData);
            log.Info($"[DB] Inserted project {projectData.Id}");

            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(projectData.Id));
            foreach (var episode in episodes)
            {
                batch.UpsertItem(episode);
            }
            await batch.ExecuteAsync();
            log.Info($"[DB] Inserted episodes for {projectData.Id}");
            log.Info("Project creation finished");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(T("project.created", lng, nickname))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            // Check progress channel permissions
            if (!PermissionChecker.CheckPermissions(updateChannelId))
                await Response.Info(T("error.missingChannelPerms", lng, $"<#{updateChannelId}>"), interaction);
            if (!PermissionChecker.CheckReleasePermissions(releaseChannelId))
                await Response.Info(T("error.missingChannelPermsRelease", lng, $"<#{releaseChannelId}>"), interaction);

            return true;
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescription("Create a new project")
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("nickname")
                .WithDescription("Project nickname")
                .WithNameLocalizations(GetOptionNames("nickname"))
                .WithDescriptionLocalizations(GetOptionDescriptions("nickname"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("title")
                .WithDescription("Full series title")
                .WithNameLocalizations(GetOptionNames("title"))
                .WithDescriptionLocalizations(GetOptionDescriptions("title"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("projecttype")
                .WithDescription("Project type")
                .WithNameLocalizations(GetOptionNames("projecttype"))
                .WithDescriptionLocalizations(GetOptionDescriptions("projecttype"))
                .WithRequired(true)
                .AddChoice("TV", 0, GetChoiceNames("projecttype-TV"))
                .AddChoice("Movie", 1, GetChoiceNames("projecttype-Movie"))
                .AddChoice("BD", 2, GetChoiceNames("projecttype-BD"))
                .WithType(ApplicationCommandOptionType.Number)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("length")
                .WithDescription("Number of episodes")
                .WithNameLocalizations(GetOptionNames("length"))
                .WithDescriptionLocalizations(GetOptionDescriptions("length"))
                .WithRequired(true)
                .WithMinValue(1)
                .WithType(ApplicationCommandOptionType.Number)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("poster")
                .WithDescription("Poster image URL")
                .WithNameLocalizations(GetOptionNames("poster"))
                .WithDescriptionLocalizations(GetOptionDescriptions("poster"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("private")
                .WithDescription("Is this project private?")
                .WithNameLocalizations(GetOptionNames("private"))
                .WithDescriptionLocalizations(GetOptionDescriptions("private"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Boolean)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("updatechannel")
                .WithDescription("Channel to post updates to")
                .WithNameLocalizations(GetOptionNames("updatechannel"))
                .WithDescriptionLocalizations(GetOptionDescriptions("updatechannel"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Channel)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("releasechannel")
                .WithDescription("Channel to post releases to")
                .WithNameLocalizations(GetOptionNames("releasechannel"))
                .WithDescriptionLocalizations(GetOptionDescriptions("releasechannel"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Channel)
            );
    }
}
