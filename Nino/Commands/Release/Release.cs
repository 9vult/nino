using Discord.Interactions;
using Fergun.Interactive;
using NLog;

namespace Nino.Commands
{
    [Group("release", "Release something to the world!")]
    public partial class Release(DataContext db, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    }
}
