using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class KeyStaff
    {
        [SlashCommand("rename", "Rename a Key Staff position")]
        public async Task<RuntimeResult> Rename(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string abbreviation,
            [Summary("newAbbreviation", "Position shorthand")] string newAbbreviation,
            [Summary("newName", "Full position name")] string newTaskName
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize imputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();
            newAbbreviation = newAbbreviation.Trim().ToUpperInvariant();
            newTaskName = newTaskName.Trim();

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Check if position exists
            if (project.KeyStaff.All(ks => ks.Role.Abbreviation != abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);
            
            // Check if position already exists
            if (abbreviation != newAbbreviation && project.KeyStaff.Any(ks => ks.Role.Abbreviation == newAbbreviation))
                return await Response.Fail(T("error.positionExists", lng), interaction);

            // Update user
            var updatedStaff = project.KeyStaff.Single(k => k.Role.Abbreviation == abbreviation);
            var ksIndex = Array.IndexOf(project.KeyStaff, updatedStaff);
            
            updatedStaff.Role.Abbreviation = newAbbreviation;
            updatedStaff.Role.Name = newTaskName;
            
            // Swap in database
            await AzureHelper.PatchProjectAsync(project, [
                PatchOperation.Replace($"/keyStaff/{ksIndex}", updatedStaff)
            ]);

            foreach (var chunk in Cache.GetEpisodes(project.Id).Chunk(50))
            {
                var batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(project));
                foreach (var e in chunk)
                {
                    var updatedTask = e.Tasks.Single(k => k.Abbreviation == abbreviation);
                    var taskIndex = Array.IndexOf(e.Tasks, updatedTask);
                
                    updatedTask.Abbreviation = newAbbreviation;
                
                    batch.PatchItem(id: e.Id.ToString(), [
                        PatchOperation.Replace($"/tasks/{taskIndex}", updatedTask),
                    ]);
                }
                await batch.ExecuteAsync();
            }

            Log.Info($"Renamed task {abbreviation} to {newAbbreviation} ({newTaskName}) in {project}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("keyStaff.renamed", lng, abbreviation, newAbbreviation, newTaskName))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(project.Id);
            return ExecutionResult.Success;
        }
    }
}
