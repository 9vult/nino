using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class KeyStaff
    {
        [SlashCommand("rename", "Rename a Key Staff position")]
        public async Task<RuntimeResult> Rename(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Autocomplete(typeof(KeyStaffAutocompleteHandler))] string abbreviation,
            string newAbbreviation,
            string newName
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();
            newAbbreviation = newAbbreviation.Trim().ToUpperInvariant();
            newName = newName.Trim();

            // Verify project and user - Owner or Admin required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (!project.VerifyUser(db, interaction.User.Id))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Check if position exists
            var staff = project.KeyStaff.SingleOrDefault(k => k.Role.Abbreviation == abbreviation);
            if (staff is null)
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Check if position already exists
            if (
                abbreviation != newAbbreviation
                && project.KeyStaff.Any(ks => ks.Role.Abbreviation == newAbbreviation)
            )
                return await Response.Fail(T("error.positionExists", lng), interaction);

            // Update user
            staff.Role.Abbreviation = newAbbreviation;
            staff.Role.Name = newName;

            foreach (var episode in project.Episodes)
            {
                var task = episode.Tasks.Single(t => t.Abbreviation == abbreviation);
                task.Abbreviation = newAbbreviation;
            }

            Log.Info($"Renamed task {abbreviation} to {newAbbreviation} ({newName}) in {project}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("keyStaff.renamed", lng, abbreviation, newAbbreviation, newName))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
