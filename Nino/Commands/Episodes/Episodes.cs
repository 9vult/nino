using Discord.Interactions;
using Fergun.Interactive;
using Nino.Handlers;
using NLog;

namespace Nino.Commands.Episodes;

[Group("episode", "Manage episodes")]
public partial class Episodes(InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
}