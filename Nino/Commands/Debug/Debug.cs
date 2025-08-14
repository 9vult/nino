using Discord.Interactions;
using Nino.Handlers;
using NLog;

namespace Nino.Commands;

[Group("debug", "Commands for debugging")]
public partial class Debug : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
}
