using Discord.Interactions;
using Nino.Handlers;
using NLog;

namespace Nino.Commands
{
    [Group("additionalstaff", "Additional staff for a single episode")]
    public partial class AdditionalStaff(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
    }
}
