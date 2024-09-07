using Discord.Interactions;
using Nino.Handlers;
using NLog;

namespace Nino.Commands
{
    [Group("project", "Project management")]
    public partial class ProjectManagement(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Group("alias", "Alternative nicknames for a project")]
        public partial class Alias(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractionService Commands { get; private set; } = commands;
            private readonly InteractionHandler _handler = handler;
            private static readonly Logger log = LogManager.GetCurrentClassLogger();
        }

        [Group("admin", "Project-level administrators")]
        public partial class Admin(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractionService Commands { get; private set; } = commands;
            private readonly InteractionHandler _handler = handler;
            private static readonly Logger log = LogManager.GetCurrentClassLogger();
        }

        [Group("airreminder", "Enable or disable airing reminders")]
        public partial class AirReminder(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractionService Commands { get; private set; } = commands;
            private readonly InteractionHandler _handler = handler;
            private static readonly Logger log = LogManager.GetCurrentClassLogger();
        }

        [Group("conga", "A Conga line of Key Staff")]
        public partial class Conga(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractionService Commands { get; private set; } = commands;
            private readonly InteractionHandler _handler = handler;
            private static readonly Logger log = LogManager.GetCurrentClassLogger();
        }
    }
}
