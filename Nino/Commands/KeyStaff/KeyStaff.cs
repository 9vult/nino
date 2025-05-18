using Discord.Interactions;
using NLog;

namespace Nino.Commands.KeyStaff;

[Group("keystaff", "Key Staff for the whole project")]
public partial class KeyStaff() : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
    [Group("pinch-hitter", "Key Staff pinch hitters")]
    public partial class PinchHitterManagement() : InteractionModuleBase<SocketInteractionContext>;
}