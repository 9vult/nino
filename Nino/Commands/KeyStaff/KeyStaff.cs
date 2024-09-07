using Discord.Interactions;
using Nino.Handlers;
using NLog;

namespace Nino.Commands
{
    [Group("keystaff", "Key Staff for the whole project")]
    public partial class KeyStaff(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
    }
}
