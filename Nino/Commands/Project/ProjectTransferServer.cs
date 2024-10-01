using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("transferserver", "Transfer a project from another server to here")]
        public async Task<RuntimeResult> TransferServer(
            [Summary("serverid", "ID of the server the project is currently in")] string serverId,
            [Summary("project", "Project nickname")] string alias
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            var originGuildIdStr = serverId.Trim();
            alias = alias.Trim();
            var newGuildId = interaction.GuildId ?? 0;

            // Check for guild administrator status
            var guild = Nino.Client.GetGuild(newGuildId);
            var member = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(member, guild)) return await Response.Fail(T("error.notPrivileged", lng), interaction);

            // Validate origin server
            if (!ulong.TryParse(originGuildIdStr, out var oldGuildId))
                return await Response.Fail(T("error.invalidServerId", lng), interaction);
            var originGuild = Nino.Client.GetGuild(oldGuildId);
            if (originGuild == null)
                return await Response.Fail(T("error.noSuchServer", lng), interaction);

            // Verify project and user - Owner required
            var project = Utils.ResolveAlias(alias, interaction, observingGuildId: oldGuildId);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Get the episodes
            var episodes = await Getters.GetEpisodes(project);

            var oldProjectId = project.Id;
            var oldEpisodeIds = episodes.Select(e => e.Id).ToList();

            // Get observers
            var observers = Cache.GetObservers(oldGuildId).Where(o => o.ProjectId == oldProjectId).ToList();
            var oldObserverIds = observers.Select(o => o.Id).ToList();

            // Modify the data
            project.Id = $"{newGuildId}-{project.Nickname}";
            project.GuildId = newGuildId;

            foreach (var episode in episodes)
            {
                episode.ProjectId = project.Id;
                episode.GuildId = newGuildId;
                episode.Id = $"{newGuildId}-{project.Nickname}-{episode.Number}";
            }

            foreach (var observer in observers)
            {
                observer.Id = $"{project.Id}-{observer.GuildId}";
                observer.OriginGuildId = newGuildId;
                observer.ProjectId = project.Id;
            }

            // Write new data to database

            await AzureHelper.Projects!.UpsertItemAsync(project);

            if (episodes.Count > 0)
            {
                TransactionalBatch episodeBatch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(project.Id));
                foreach (var episode in episodes)
                {
                    episodeBatch.UpsertItem(episode);
                }
                await episodeBatch.ExecuteAsync();
            }

            if (observers.Count > 0)
            {
                TransactionalBatch observerBatch = AzureHelper.Observers!.CreateTransactionalBatch(partitionKey: AzureHelper.ObserverPartitionKey(newGuildId));
                foreach (var observer in observers)
                {
                    observerBatch.UpsertItem(observer);
                }
                await observerBatch.ExecuteAsync();
            }

            // Remove old data from database
            await AzureHelper.Projects!.DeleteItemAsync<Project>(oldProjectId, partitionKey: new PartitionKey(oldGuildId.ToString()));

            if (episodes.Count > 0)
            {
                TransactionalBatch episodeBatch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(oldProjectId));
                foreach (var id in oldEpisodeIds)
                {
                    episodeBatch.DeleteItem(id);
                }
                await episodeBatch.ExecuteAsync();
            }

            // Remove observers from database
            if (observers.Count > 0)
            {
                TransactionalBatch observerBatch = AzureHelper.Observers!.CreateTransactionalBatch(partitionKey: new PartitionKey(oldGuildId.ToString()));
                foreach (var id in oldObserverIds)
                {
                    observerBatch.DeleteItem(id);
                }
                await observerBatch.ExecuteAsync();
            }


            log.Info($"Transfered project {oldProjectId} to new server {newGuildId}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("project.server.transferred", lng, project.Nickname))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.BuildCache();
            return ExecutionResult.Success;
        }
    }
}
