using Discord.Interactions;
using NLog;

namespace Nino.Commands
{
    [Group("additionalstaff", "Additional staff for a single episode")]
    public partial class AdditionalStaff(DataContext db)
        : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    }
}
