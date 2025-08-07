using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class KeyStaff
    {
        [SlashCommand("set-weight", "Set the weight of a Key Staff position")]
        public async Task<RuntimeResult> SetWeight(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string abbreviation,
            [Summary("weight", "Weight")] decimal inputWeight
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();

            // Verify project and user - Owner or Admin required
            var project = db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Check if position exists
            var staff = project.KeyStaff.SingleOrDefault(s => s.Role.Abbreviation == abbreviation);
            if (staff is null)
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Update user
            staff.Role.Weight = inputWeight;
            
            Log.Info($"Set {abbreviation} weight to {inputWeight} in {project}");

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
