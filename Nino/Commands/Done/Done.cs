using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Done(InteractionHandler handler, InteractionService commands, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private readonly InteractiveService _interactiveService = interactive;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [SlashCommand("done", "Mark a position as done")]
        public async Task<RuntimeResult> Handle(
            [Summary("project", "Project nickname")] string alias,
            [Summary("abbreviation", "Position shorthand")] string abbreviation,
            [Summary("episode", "Episode number")] decimal? episodeNumber = null
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();
            
            // Verify project
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (episodeNumber != null)
                return await HandleSpecified(interaction, project, abbreviation, (decimal)episodeNumber);
            else
                return await HandleUnspecified(interaction, project, abbreviation, _interactiveService);
        }
    }
}
