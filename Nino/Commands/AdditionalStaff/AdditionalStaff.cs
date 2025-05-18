using Discord.Interactions;
using Nino.Handlers;
using NLog;

namespace Nino.Commands.AdditionalStaff;

[Group("additionalstaff", "Additional staff for a single episode")]
public partial class AdditionalStaff() : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
}