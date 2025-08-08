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
        [SlashCommand("rename", "Rename an additional staff position")]
        public async Task<RuntimeResult> Rename(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
            [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(AdditionalStaffAutocompleteHandler))] string abbreviation,
            [Summary("newAbbreviation", "Position shorthand")] string newAbbreviation,
            [Summary("newName", "Full position name")] string newTaskName
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            newTaskName = newTaskName.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();
            newAbbreviation = newAbbreviation.Trim().ToUpperInvariant();
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
            
            // Check if position already exists
            if (abbreviation != newAbbreviation && episode.Tasks.Any(t => t.Abbreviation == newAbbreviation))
                return await Response.Fail(T("error.positionExists", lng), interaction);

            // Update user
            var staff = episode.AdditionalStaff.Single(k => k.Role.Abbreviation == abbreviation);
            var task = episode.Tasks.Single(k => k.Abbreviation == abbreviation);
            
            staff.Role.Abbreviation = newAbbreviation;
            staff.Role.Name = newTaskName;
            task.Abbreviation = newAbbreviation;

            Log.Info($"Renamed task {abbreviation} for episode {episode} to {newAbbreviation} ({newTaskName})");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(T("additionalStaff.renamed", lng, abbreviation, episode.Number, newAbbreviation, newTaskName))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
