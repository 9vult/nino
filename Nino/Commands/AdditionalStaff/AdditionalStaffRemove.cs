using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class AdditionalStaff
    {
        [SlashCommand("remove", "Remove additional staff from an episode")]
        public async Task<RuntimeResult> Remove(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
            [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(AdditionalStaffAutocompleteHandler))] string abbreviation,
            [Summary("allepisodes", "Remove this position from all episodes that have it?"), Autocomplete(typeof(AdditionalStaffAutocompleteHandler))] bool allEpisodes = false
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize imputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();
            episodeNumber = Utils.CanonicalizeEpisodeNumber(episodeNumber);

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            var episode = await Getters.GetEpisode(project, episodeNumber);
            if (episode == null)
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

            // Check if position exists
            if (!episode.AdditionalStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Remove from database
            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(episode));

            if (allEpisodes)
            {
                foreach (Episode e in await Getters.GetEpisodes(project))
                {
                    if (!e.AdditionalStaff.Any(k => k.Role.Abbreviation == abbreviation)) continue;

                    var asIndex = Array.IndexOf(e.AdditionalStaff, e.AdditionalStaff.Single(k => k.Role.Abbreviation == abbreviation));
                    var taskIndex = Array.IndexOf(e.Tasks, e.Tasks.Single(t => t.Abbreviation == abbreviation));

                    batch.PatchItem(id: e.Id, [
                        PatchOperation.Remove($"/additionalStaff/{asIndex}"),
                        PatchOperation.Remove($"/tasks/{taskIndex}")
                    ]);
                }
            }  
            else
            {
                var asIndex = Array.IndexOf(episode.AdditionalStaff, episode.AdditionalStaff.Single(k => k.Role.Abbreviation == abbreviation));
                var taskIndex = Array.IndexOf(episode.Tasks, episode.Tasks.Single(t => t.Abbreviation == abbreviation));
                batch.PatchItem(id: episode.Id, [
                    PatchOperation.Remove($"/additionalStaff/{asIndex}"),
                    PatchOperation.Remove($"/tasks/{taskIndex}")
                ]);
            }

            await batch.ExecuteAsync();

            if (allEpisodes) log.Info($"Removed {abbreviation} from {episode.Id}");
            else log.Info($"Removed additionalstaff {abbreviation} from {episode.ProjectId}");

            var description = allEpisodes 
                ? T("additionalStaff.removed.all", lng, abbreviation) 
                : T("additionalStaff.removed", lng, abbreviation, episode.Number);

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(description)
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(episode.ProjectId);
            return ExecutionResult.Success;
        }
    }
}
