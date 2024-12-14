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
        public partial class PinchHitterManagement
        {
            [SlashCommand("remove", "Remove a pinch hitter from an episode")]
            public async Task<RuntimeResult> Remove(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
                [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string abbreviation
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
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
                if (!Getters.TryGetEpisode(project, episodeNumber, out var episode))
                    return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

                // Check if position exists
                if (project.KeyStaff.All(ks => ks.Role.Abbreviation != abbreviation))
                    return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

                // Remove from database
                TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(episode));

                var phIndex = Array.IndexOf(episode.PinchHitters, episode.PinchHitters.SingleOrDefault(k => k.Abbreviation == abbreviation));
                if (phIndex < 0)
                    return await Response.Fail(T("error.noSuchPinchHitter", lng, abbreviation), interaction);
                
                batch.PatchItem(id: episode.Id.ToString(), [
                    PatchOperation.Remove($"/pinchHitters/{phIndex}")
                ]);
                await batch.ExecuteAsync();
                log.Info($"Removed pinch hitter for {abbreviation} from {episode.Id}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("keyStaff.pinchHitter.removed", lng, episode.Number, abbreviation))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await Cache.RebuildCacheForProject(episode.ProjectId);
                return ExecutionResult.Success;
            }
        }
    }
}
