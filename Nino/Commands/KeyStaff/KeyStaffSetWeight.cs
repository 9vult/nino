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
        [SlashCommand("set-weight", "Set the weight of a Key Staff position")]
        public async Task<RuntimeResult> SetWeight(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Autocomplete(typeof(KeyStaffAutocompleteHandler))] string abbreviation,
            decimal weight
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();

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
            var staff = project.KeyStaff.SingleOrDefault(s => s.Role.Abbreviation == abbreviation);
            if (staff is null)
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Update user
            staff.Role.Weight = weight;

            Log.Info($"Set {abbreviation} weight to {weight} in {project}");

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
