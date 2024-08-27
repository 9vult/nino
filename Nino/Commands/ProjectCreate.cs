using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

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

            // Get inputs
            var nickname = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "nickname")!.Value).Trim();
            var title = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "title")!.Value).Trim();
            var type = (ProjectType)Convert.ToInt32(subcommand.Options.FirstOrDefault(o => o.Name == "type")!.Value);
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
                AirReminderEnabled = false,
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

            log.Info($"Creating project {projectData.Id} for {projectData.OwnerId} with {episodes.Count} episodes");

            // Add project and episodes to database
            await AzureHelper.Projects!.UpsertItemAsync(projectData);

            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(projectData.Id));
            foreach (var episode in episodes)
            {
                batch.UpsertItem(episode);
            }
            await batch.ExecuteAsync();

            // Create configuration if the guild doesn't have one yet
            if (await Getters.GetConfiguration(guildId) == null)
            {
                log.Info($"Creating default configuration for guild {guildId}");
                await AzureHelper.Configurations!.UpsertItemAsync(Configuration.CreateDefault(guildId));
            }

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

            await Cache.RebuildCacheForGuild(interaction.GuildId ?? 0);
            return true;
        }
    }
}
