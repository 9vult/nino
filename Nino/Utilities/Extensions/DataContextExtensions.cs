using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Nino.Records;
using NLog;
using Task = System.Threading.Tasks.Task;

namespace Nino.Utilities.Extensions;

public static class DataContextExtensions
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Resolve an alias to its project
    /// </summary>
    /// <param name="db">DataContext</param>
    /// <param name="query">Alias to resolve</param>
    /// <param name="interaction">Interaction requesting resolution</param>
    /// <param name="observingGuildId">ID of the build being observed, if applicable</param>
    /// <param name="includeObservers">Whether to include observers in the lookup</param>
    /// <returns>Project the alias references to, or null</returns>
    public static async Task<Project?> ResolveAlias(
        this DataContext db,
        string query,
        SocketInteraction interaction,
        ulong? observingGuildId = null,
        bool includeObservers = false
    )
    {
        var guildId = observingGuildId ?? interaction.GuildId;

        var result = await db
            .Projects.Include(p => p.Episodes)
            .Where(p =>
                p.GuildId == guildId
                || (includeObservers && p.Observers.Any(o => o.GuildId == guildId))
            )
            .FirstOrDefaultAsync(p => p.Nickname == query || p.Aliases.Any(a => a.Value == query));

        Log.Trace($"Resolved alias {query} to {result?.ToString() ?? "<resolution failed>"}");

        if (!result?.IsPrivate ?? true)
            return result;
        
        // Verify the user has permission to view the project
        if (result.VerifyUser(db, interaction.User.Id, includeStaff: true))
            return result;
        
        Log.Trace($"Query for {result} rejected due to insufficient permissions");
        return null;
    }

    public static Configuration? GetConfig(this DataContext db, ulong guildId)
    {
        return db.Configurations.FirstOrDefault(c => c.GuildId == guildId);
    }

    public static async Task TrySaveChangesAsync(this DataContext db, SocketInteraction interaction)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            await Response.Fail(
                $"Your changes were not saved:\n{ex.Message}\n\nPlease report this to <@{Nino.Config.OwnerId}>!",
                interaction
            );
        }
    }
}
