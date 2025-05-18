using Discord.Interactions;
using Fergun.Interactive;
using NLog;

namespace Nino.Commands.Release;

[Group("release", "Release something to the world!")]
public partial class Release(InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
}