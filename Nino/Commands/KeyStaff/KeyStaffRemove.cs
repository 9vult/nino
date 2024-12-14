using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class KeyStaff
    {
        [SlashCommand("remove", "Remove a Key Staff from the whole project")]
        public async Task<RuntimeResult> Remove(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string abbreviation
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize imputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Check if position exists
            if (project.KeyStaff.All(ks => ks.Role.Abbreviation != abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Remove from database
            var ksIndex = Array.IndexOf(project.KeyStaff, project.KeyStaff.Single(k => k.Role.Abbreviation == abbreviation));
            await AzureHelper.PatchProjectAsync(project, [
                PatchOperation.Remove($"/keyStaff/{ksIndex}")
            ]);

            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(project));
            foreach (var e in Cache.GetEpisodes(project.Id))
            {
                var taskIndex = Array.IndexOf(e.Tasks, e.Tasks.Single(t => t.Abbreviation == abbreviation));
                batch.PatchItem(id: e.Id.ToString(), [
                    PatchOperation.Remove($"/tasks/{taskIndex}")
                ]);
            }
            await batch.ExecuteAsync();

            log.Info($"Removed {abbreviation} from {project.Id}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("keyStaff.removed", lng, abbreviation))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(project.Id);
            return ExecutionResult.Success;
        }
    }
}
