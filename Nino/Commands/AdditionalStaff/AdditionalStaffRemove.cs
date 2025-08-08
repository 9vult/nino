using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;
using Nino.Utilities.Extensions;
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
            [Summary("allEpisodes", "Remove this position from all episodes that have it?"), Autocomplete(typeof(AdditionalStaffAutocompleteHandler))] bool allEpisodes = false
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();
            episodeNumber = Episode.CanonicalizeEpisodeNumber(episodeNumber);

            // Verify project and user - Owner or Admin required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!project.VerifyUser(db, interaction.User.Id))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            if (!project.TryGetEpisode(episodeNumber, out var episode))
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

            // Check if position exists
            if (episode.AdditionalStaff.All(ks => ks.Role.Abbreviation != abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Remove from database
            if (allEpisodes)
            {
                foreach (var e in project.Episodes)
                {
                    e.AdditionalStaff.RemoveAll(s => s.Role.Abbreviation == abbreviation);
                    e.Tasks.RemoveAll(t => t.Abbreviation == abbreviation);
                    e.Done = e.Tasks.All(t => t.Done);
                }
            }  
            else
            {
                episode.AdditionalStaff.RemoveAll(s => s.Role.Abbreviation == abbreviation);
                episode.Tasks.RemoveAll(t => t.Abbreviation == abbreviation);
                episode.Done = episode.Tasks.All(t => t.Done);
            }


            Log.Info(allEpisodes
                ? $"Removed {abbreviation} from {episode}"
                : $"Removed additionalStaff {abbreviation} from {project}");

            var description = allEpisodes 
                ? T("additionalStaff.removed.all", lng, abbreviation) 
                : T("additionalStaff.removed", lng, abbreviation, episode.Number);

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(description)
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
