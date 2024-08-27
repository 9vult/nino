using Microsoft.Azure.Cosmos;
using Nino.Records;
using NLog;

namespace Nino.Utilities
{
    internal static class Cache
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<ulong, CachedGuild> _guildCache = [];
        private static readonly Dictionary<ulong, List<CachedProject>> _projectCache = [];
        private static readonly Dictionary<string, List<CachedEpisode>> _episodeCache = [];

        /// <summary>
        /// Get a cached guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <returns>Cached guild</returns>
        public static CachedGuild? GetGuild(ulong guildId)
        {
            if (_guildCache.TryGetValue(guildId, out var cachedGuild)) 
                return cachedGuild;
            return null;
        }

        /// <summary>
        /// Get a flattened list of all cached projects
        /// </summary>
        /// <returns>List of all projects</returns>
        public static List<CachedProject> GetProjects()
        {
            return _projectCache.Values.SelectMany(list => list).ToList();
        }

        /// <summary>
        /// Get the cached projects for a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <returns>The projects, or an empty list</returns>
        public static List<CachedProject> GetProjects(ulong guildId)
        {
            if (_projectCache.TryGetValue(guildId, out var projectCache))
                return projectCache;
            return [];
        }

        /// <summary>
        /// Get a flattened list of all cached episodes
        /// </summary>
        /// <returns>List of all episodes</returns>
        public static List<CachedEpisode> GetEpisodes()
        {
            return _episodeCache.Values.SelectMany(list => list).ToList();
        }

        /// <summary>
        /// Get the cached episodes for a project
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <returns>The episodes, or an empty list</returns>
        public static List<CachedEpisode> GetEpisodes(string projectId)
        {
            if (_episodeCache.TryGetValue(projectId, out var episodeCache))
                return episodeCache;
            return [];
        }

        /// <summary>
        /// Rebuild the project cache for a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        public static async System.Threading.Tasks.Task RebuildCacheForGuild(ulong guildId)
        {
            log.Info($"Rebuilding cache for guild {guildId}...");
            var projectSql = new QueryDefinition("SELECT * FROM c WHERE c.guildId = @guildId")
                .WithParameter("@guildId", guildId.ToString());

            var episodeSql = new QueryDefinition("SELECT c.number, c.projectId, c.tasks FROM c WHERE c.guildId = @guildId")
                .WithParameter("@guildId", guildId.ToString());

            // Get data
            List<Project> rawProjects = await AzureHelper.QueryProjects<Project>(projectSql);
            List<CachedEpisode> allEpisodes = await AzureHelper.QueryEpisodes<CachedEpisode>(episodeSql);

            // Transform data
            List<CachedProject> cachedProjects = [];
            foreach (var project in rawProjects)
            {
                var episodes = allEpisodes.Where(e => e.ProjectId == project.Id).ToList();
                _episodeCache[project.Id] = episodes;

                cachedProjects.Add(new CachedProject
                {
                    Id = project.Id,
                    GuildId = project.GuildId,
                    Nickname = project.Nickname,
                    OwnerId = project.OwnerId,
                    Aliases = project.Aliases,
                    AdministratorIds = project.AdministratorIds,
                    KeyStaffAbbreviations = project.KeyStaff.Select(ks => ks.Role.Abbreviation).ToArray(),
                    IsPrivate = project.IsPrivate
                });
            }
            _projectCache[guildId] = cachedProjects;
            log.Info($"Cache for guild {guildId} successfully rebuilt: {cachedProjects.Count} projects, {allEpisodes.Count} episodes");
        }

        /// <summary>
        /// Build the cache
        /// </summary>
        public static async System.Threading.Tasks.Task BuildCache()
        {
            log.Info("Building cache...");
            var sql = new QueryDefinition("SELECT DISTINCT c.guildId FROM c");
            var response = await AzureHelper.QueryProjects<dynamic>(sql);

            foreach (var item in response)
            {
                var guildId = ulong.Parse((string)item.guildId);
                await RebuildCacheForGuild(guildId);
            }

            log.Info("Cache successfully built");
        }
    }

    /// <summary>
    /// Minimal guild info structure for caching purposes
    /// </summary>
    internal record CachedGuild
    {
        public required ulong Id;
        public required ulong[] Administrators;
    }

    /// <summary>
    /// Minimal project structure for caching purposes
    /// </summary>
    internal record CachedProject
    {
        public required string Id;
        public required ulong GuildId;
        public required string Nickname;
        public required ulong OwnerId;
        public required string[] Aliases;
        public required ulong[] AdministratorIds;
        public required string[] KeyStaffAbbreviations;
        public required bool IsPrivate;
    }

    /// <summary>
    /// Minimal episode structure for caching purposes
    /// </summary>
    internal record CachedEpisode
    {
        public required decimal Number;
        public required string ProjectId;
        public required Records.Task[] Tasks;
    }
}
