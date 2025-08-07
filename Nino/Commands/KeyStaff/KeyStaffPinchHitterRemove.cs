using Discord;
using Discord.Interactions;
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
                var project = db.ResolveAlias(alias, interaction);
                if (project is null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Verify episode
                if (!project.TryGetEpisode(episodeNumber, out var episode))
                    return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

                // Check if position exists
                if (project.KeyStaff.All(ks => ks.Role.Abbreviation != abbreviation))
                    return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

                // Remove from database
                
                var ph = episode.PinchHitters.FirstOrDefault(p => p.Abbreviation == abbreviation);
                if (ph is null)
                    return await Response.Fail(T("error.noSuchPinchHitter", lng, abbreviation), interaction);
                
                episode.PinchHitters.Remove(ph);
                Log.Info($"Removed pinch hitter for {abbreviation} from {episode}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("keyStaff.pinchHitter.removed", lng, episode.Number, abbreviation))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await db.TrySaveChangesAsync(interaction);
                return ExecutionResult.Success;
            }
        }
    }
}
