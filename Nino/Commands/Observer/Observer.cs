using Discord.Interactions;
using NLog;

namespace Nino.Commands
{
    [Group("observer", "Observe a project on another server")]
    public partial class Observer(DataContext db) : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    }
}
