﻿using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Services;
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
            
            // Ask if the user is sure
            var (goOn, finalBody) = await Ask.AboutIrreversibleAction(_interactiveService, interaction, project, lng,
                Ask.IrreversibleAction.Delete);

            if (!goOn)
            {
                var cancelEmbed = new EmbedBuilder()
                    .WithTitle(T("title.projectDeletion", lng))
                    .WithDescription(finalBody)
                    .Build();
                await interaction.ModifyOriginalResponseAsync(m => {
                    m.Embed = cancelEmbed;
                    m.Components = null;
                });
                return ExecutionResult.Success;
            }
            
            Log.Info($"Exporting project {project} before deletion");

            // Get stream
            var file = ExportService.ExportProject(project, false);

            // Respond
            await interaction.FollowupWithFileAsync(file, $"{project.Id}.json", T("project.exported", lng, project.Nickname));

            Log.Info($"Deleting project {project}");

            // Remove from database
            await AzureHelper.Projects!.DeleteItemAsync<Project>(project.Id.ToString(), partitionKey: AzureHelper.ProjectPartitionKey(project));

            foreach (var chunk in Cache.GetEpisodes(project.Id).Chunk(50))
            {
                var episodeBatch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(project.Id.ToString()));
                foreach (var e in chunk)
                {
                    episodeBatch.DeleteItem(e.Id.ToString());
                }
                await episodeBatch.ExecuteAsync();
            }

            // Remove observers from database
            var observers = Cache.GetObservers(project.GuildId);
            if (observers.Count > 0)
            {
                foreach (var chunk in observers.Chunk(50))
                {
                    var observerBatch = AzureHelper.Observers!.CreateTransactionalBatch(partitionKey: new PartitionKey(project.GuildId));
                    foreach (var o in chunk)
                    {
                        observerBatch.DeleteItem(o.Id.ToString());
                    }
                    await observerBatch.ExecuteAsync();
                }
            }

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectDeletion", lng))
                .WithDescription(T("project.deleted", lng, project.Title))
                .Build();
            await interaction.ModifyOriginalResponseAsync(m => {
                m.Embed = embed;
                m.Components = null;
            });

            await Cache.RebuildCacheForGuild(interaction.GuildId ?? 0);
            await Cache.RebuildObserverCache();
            return ExecutionResult.Success;
        }
    }
}
