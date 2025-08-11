using Discord.Interactions;
using Fergun.Interactive;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Done(DataContext db, InteractiveService interactive)
        : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [SlashCommand("done", "Mark a position as done")]
        public async Task<RuntimeResult> Handle(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Autocomplete(typeof(AbbreviationAutocompleteHandler))] string abbreviation,
            [Autocomplete(typeof(EpisodeAutocompleteHandler))] string? episodeNumber = null
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();

            // Verify project
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (project.IsArchived)
                return await Response.Fail(T("error.archived", lng), interaction);

            // Check progress channel permissions
            var goOn = await PermissionChecker.Precheck(interactive, interaction, project, lng);
            // Cancel
            if (!goOn)
                return ExecutionResult.Success;

            // Check Conga permissions
            if (project.CongaParticipants.Nodes.Count != 0)
            {
                goOn = await PermissionChecker.Precheck(
                    interactive,
                    interaction,
                    project,
                    lng,
                    false,
                    true
                );
                // Cancel
                if (!goOn)
                    return ExecutionResult.Success;
            }

            if (episodeNumber != null)
                return await HandleSpecified(
                    interaction,
                    project,
                    abbreviation,
                    Episode.CanonicalizeEpisodeNumber(episodeNumber)
                );
            return await HandleUnspecified(interaction, project, abbreviation, interactive);
        }
    }
}
