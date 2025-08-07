using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class AdditionalStaff
    {
        [SlashCommand("set-weight", "Set the weight of an Additional Staff position")]
        public async Task<RuntimeResult> SetWeight(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
            [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(AdditionalStaffAutocompleteHandler))] string abbreviation,
            [Summary("weight", "Weight")] decimal inputWeight
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
            if (episode.AdditionalStaff.All(ks => ks.Role.Abbreviation != abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Update user
            var staff = episode.AdditionalStaff.Single(k => k.Role.Abbreviation == abbreviation);
            staff.Role.Weight = inputWeight;

            Log.Info($"Set {abbreviation} weight to {inputWeight} in {episode}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("keyStaff.weight.updated", lng, abbreviation, inputWeight))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
