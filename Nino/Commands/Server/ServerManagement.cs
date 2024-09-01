using Discord.Interactions;
using Nino.Handlers;
using NLog;

namespace Nino.Commands
{
    [Group("server", "Server-wide options")]
    public partial class ServerManagement(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Group("display", "Control the look and features of embeds")]
        public partial class Display(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractionService Commands { get; private set; } = commands;
            private readonly InteractionHandler _handler = handler;
            private static readonly Logger log = LogManager.GetCurrentClassLogger();
        }

        [Group("admin", "Server-level administrators")]
        public partial class Admin(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractionService Commands { get; private set; } = commands;
            private readonly InteractionHandler _handler = handler;
            private static readonly Logger log = LogManager.GetCurrentClassLogger();
        }
    }
}
