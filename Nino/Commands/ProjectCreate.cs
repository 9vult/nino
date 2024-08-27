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
using Nino.Records.Enums;

namespace Nino.Commands
{
    internal static partial class ProjectManagement
    {
        public static async Task<bool> HandleCreate(SocketSlashCommand interaction)
        {
            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);
            var member = guild.GetUser(interaction.User.Id);
            var lng = interaction.UserLocale;
            if (!member.GuildPermissions.Administrator) return await Response.Fail(T("error.notPrivileged", lng), interaction);

            var subcommand = interaction.Data.Options.First();

            log.Info("Beginning project creation");

            // Get inputs
            var nickname = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "nickname")!.Value).Trim();
            var title = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "title")!.Value).Trim();
            var type = (ProjectType)Convert.ToInt32(subcommand.Options.FirstOrDefault(o => o.Name == "projecttype")!.Value);
            var length = Convert.ToInt32(subcommand.Options.FirstOrDefault(o => o.Name == "length")!.Value);
            var posterUri = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "poster")!.Value).Trim();
            var isPrivate = (bool)subcommand.Options.FirstOrDefault(o => o.Name == "private")!.Value;
            var updateChannelId = ((SocketChannel)subcommand.Options.FirstOrDefault(o => o.Name == "updatechannel")!.Value).Id;
            var releaseChannelId = ((SocketChannel)subcommand.Options.FirstOrDefault(o => o.Name == "releasechannel")!.Value).Id;
            var ownerId = interaction.User.Id;

            // Verify data
            // TODO: if name exists, reject

            // Populate data

            var projectData = new Records.Project
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
                    GuildId = guildId,
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
    }
}
