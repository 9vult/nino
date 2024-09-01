using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;
using NLog;

using static Localizer.Localizer;

namespace Nino.Commands
{
    [Group("episode", "Manage episodes")]
    public partial class Episodes(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
    }
}
