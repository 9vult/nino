using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Nino.Records;
using NLog;
using Task = System.Threading.Tasks.Task;

namespace Nino.Utilities;

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
    public static Project? ResolveAlias(
        this DataContext db,
        string query,
        SocketInteraction interaction,
        ulong? observingGuildId = null,
        bool includeObservers = false
    )
    {
        var guildId = observingGuildId ?? interaction.GuildId;

        var q = db.Projects
            .Include(p => p.Episodes)
            .ConcatIf(
                includeObservers,
                db.Observers.Where(o => o.GuildId == guildId).Select(o => o.Project)
            )
            .AsQueryable();

        var result = q.AsEnumerable().FirstOrDefault(p =>
            string.Equals(p.Nickname, query, StringComparison.InvariantCultureIgnoreCase)
            || p.Aliases.Any(a =>
                string.Equals(a, query, StringComparison.InvariantCultureIgnoreCase)
            )
        );

        Log.Trace($"Resolved alias {query} to {result?.ToString() ?? "<resolution failed>"}");
        return result;
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
            await Response.Fail($"Your changes were not saved:\n{ex.Message}\n\nPlease report this to <@{Nino.Config.OwnerId}>!", interaction);
        }
    }
}
