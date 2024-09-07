using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("delete", "Delete a project")]
        public async Task<RuntimeResult> Delete(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Verify project and user - Owner required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            log.Info($"Deleting project {project.Id}");

            // Remove from database
            await AzureHelper.Projects!.DeleteItemAsync<Project>(project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project));

            TransactionalBatch episodeBatch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(project.Id));
            foreach (Episode e in await Getters.GetEpisodes(project))
            {
                episodeBatch.DeleteItem(e.Id);
            }
            await episodeBatch.ExecuteAsync();

            // Remove observers from database
            var observers = Cache.GetObservers(project.GuildId);
            if (observers.Count > 0)
            {
                TransactionalBatch observerBatch = AzureHelper.Observers!.CreateTransactionalBatch(partitionKey: new PartitionKey(project.GuildId));
                foreach (Records.Observer o in observers)
                {
                    observerBatch.DeleteItem(o.Id);
                }
                await observerBatch.ExecuteAsync();
            }

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectDeletion", lng))
                .WithDescription(T("project.deleted", lng, project.Title))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForGuild(interaction.GuildId ?? 0);
            await Cache.RebuildObserverCache();
            return ExecutionResult.Success;
        }
    }
}
