using Discord.Interactions;
using Nino.Handlers;
using NLog;

namespace Nino.Commands
{
    [Group("observer", "Observe a project on another server")]
    public partial class Observer(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    }
}
