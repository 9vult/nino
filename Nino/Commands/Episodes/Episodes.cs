using Discord.Interactions;
using Fergun.Interactive;
using NLog;

namespace Nino.Commands
{
    [Group("episode", "Manage episodes")]
    public partial class Episodes(DataContext db, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    }
}
