using Discord.Interactions;
using Fergun.Interactive;
using NLog;

namespace Nino.Commands;

[Group("keystaff", "Key Staff for the whole project")]
public partial class KeyStaff(DataContext db, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
    [Group("pinch-hitter", "Key Staff pinch hitters")]
    public partial class PinchHitterManagement(DataContext db) : InteractionModuleBase<SocketInteractionContext>;
}