using Discord.Interactions;
using Fergun.Interactive;
using NLog;

namespace Nino.Commands
{
    [Group("project", "Project management")]
    public partial class ProjectManagement(DataContext db, InteractiveService interactive)
        : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Group("alias", "Alternative nicknames for a project")]
        public partial class Alias(DataContext db)
            : InteractionModuleBase<SocketInteractionContext>;

        [Group("admin", "Project-level administrators")]
        public partial class Admin(DataContext db)
            : InteractionModuleBase<SocketInteractionContext>;

        [Group("air-reminder", "Enable or disable airing reminders")]
        public partial class AirReminder(DataContext db)
            : InteractionModuleBase<SocketInteractionContext>;

        [Group("conga", "A Conga line of staff!")]
        public partial class Conga(DataContext db)
            : InteractionModuleBase<SocketInteractionContext>;

        [Group("conga-reminder", "Enable or disable conga reminders")]
        public partial class CongaReminder(DataContext db)
            : InteractionModuleBase<SocketInteractionContext>;
    }
}
