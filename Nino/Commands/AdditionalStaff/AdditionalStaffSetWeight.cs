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
        [SlashCommand("set-weight", "Set the weight of an Additional Staff position")]
        public async Task<RuntimeResult> SetWeight(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
            [Autocomplete(typeof(AdditionalStaffAutocompleteHandler))] string abbreviation,
            decimal weight
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
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (!project.VerifyUser(db, interaction.User.Id))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            if (!project.TryGetEpisode(episodeNumber, out var episode))
                return await Response.Fail(
                    T("error.noSuchEpisode", lng, episodeNumber),
                    interaction
                );

            // Check if position exists
            if (episode.AdditionalStaff.All(ks => ks.Role.Abbreviation != abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Update user
            var staff = episode.AdditionalStaff.Single(k => k.Role.Abbreviation == abbreviation);
            staff.Role.Weight = weight;

            Log.Info($"Set {abbreviation} weight to {weight} in {episode}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("keyStaff.weight.updated", lng, abbreviation, weight))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
