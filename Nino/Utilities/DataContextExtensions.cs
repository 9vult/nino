using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Nino.Records;
using NLog;

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
    /// <param name="includeEpisodes">Whether to include episodes in the resulting project</param>
    /// <returns>Project the alias references to, or null</returns>
    public static Project? ResolveAlias(
        this DataContext db,
        string query,
        SocketInteraction interaction,
        ulong? observingGuildId = null,
        bool includeObservers = false,
        bool includeEpisodes = true
    )
    {
        var guildId = observingGuildId ?? interaction.GuildId;

        var q = db
            .Projects.ConcatIf(
                includeObservers,
                db.Observers.Where(o => o.GuildId == guildId).Select(o => o.Project)
            )
            .AsQueryable();

        if (includeEpisodes)
            q = q.Include(p => p.Episodes);

        var result = q.FirstOrDefault(p =>
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
}
