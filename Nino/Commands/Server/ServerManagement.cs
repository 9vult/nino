using Discord.Interactions;
using NLog;

namespace Nino.Commands
{
    [Group("server", "Server-wide options")]
    public partial class ServerManagement(DataContext db)
        : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Group("display", "Control the look and features of embeds")]
        public partial class Display(DataContext db)
            : InteractionModuleBase<SocketInteractionContext>;

        [Group("admin", "Server-level administrators")]
        public partial class Admin(DataContext db)
            : InteractionModuleBase<SocketInteractionContext>;
    }
}
