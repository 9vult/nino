using Discord.Interactions;
using Nino.Handlers;
using NLog;

namespace Nino.Commands;

[Group("debug", "Commands for debugging")]
public partial class Debug(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService Commands { get; private set; } = commands;
    private readonly InteractionHandler _handler = handler;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
}
