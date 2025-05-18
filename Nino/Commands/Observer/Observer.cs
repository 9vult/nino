using Discord.Interactions;
using NLog;

namespace Nino.Commands.Observer;

[Group("observer", "Observe a project on another server")]
public partial class Observer() : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
}