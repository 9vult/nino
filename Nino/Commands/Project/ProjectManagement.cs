using Discord.Interactions;
using Fergun.Interactive;
using Nino.Handlers;
using NLog;

namespace Nino.Commands
{
    [Group("project", "Project management")]
    public partial class ProjectManagement(InteractionHandler handler, InteractionService commands, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private readonly InteractiveService _interactiveService = interactive;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Group("alias", "Alternative nicknames for a project")]
        public partial class Alias(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractionService Commands { get; private set; } = commands;
            private readonly InteractionHandler _handler = handler;
        }

        [Group("admin", "Project-level administrators")]
        public partial class Admin(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractionService Commands { get; private set; } = commands;
            private readonly InteractionHandler _handler = handler;
        }

        [Group("airreminder", "Enable or disable airing reminders")]
        public partial class AirReminder(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractionService Commands { get; private set; } = commands;
            private readonly InteractionHandler _handler = handler;
        }

        [Group("conga", "A Conga line of Key Staff")]
        public partial class Conga(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractionService Commands { get; private set; } = commands;
            private readonly InteractionHandler _handler = handler;
        }
    }
}
