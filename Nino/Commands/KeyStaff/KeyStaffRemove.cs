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
        [SlashCommand("remove", "Remove a Key Staff from the whole project")]
        public async Task<RuntimeResult> Remove(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string abbreviation
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
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!project.VerifyUser(db, interaction.User.Id))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Check if position exists
            var ks = project.KeyStaff.FirstOrDefault(k => k.Role.Abbreviation == abbreviation);
            if (ks is null)
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            project.KeyStaff.Remove(ks);

            foreach (var episode in project.Episodes)
            {
                episode.Tasks.RemoveAll(t => t.Abbreviation == abbreviation);
                episode.Done = episode.Tasks.All(t => t.Done);
            }

            Log.Info($"Removed {abbreviation} from {project}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("keyStaff.removed", lng, abbreviation))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
