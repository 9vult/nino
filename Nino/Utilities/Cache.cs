using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Records.Enums;
using NLog;

namespace Nino.Utilities
{
    internal static class Cache
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<ulong, List<Project>> _projectCache = [];
        private static readonly Dictionary<ulong, CachedConfig> _configCache = [];
        private static readonly Dictionary<string, List<CachedEpisode>> _episodeCache = [];

        /// <summary>
        /// Get a cached config
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <returns>Cached config</returns>
        public static CachedConfig? GetConfig(ulong guildId)
        {
            if (_configCache.TryGetValue(guildId, out var cachedGuild))
                return cachedGuild;
            return null;
        }

        /// <summary>
        /// Get a flattened list of all cached projects
        /// </summary>
        /// <returns>List of all projects</returns>
        public static List<Project> GetProjects()
        {
            return _projectCache.Values.SelectMany(list => list).ToList();
        }

        /// <summary>
        /// Get the cached projects for a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <returns>The projects, or an empty list</returns>
        public static List<Project> GetProjects(ulong guildId)
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
        /// Rebuild the project and episode cache for a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        public static async System.Threading.Tasks.Task RebuildCacheForGuild(ulong guildId)
        {
            log.Info($"Rebuilding cache for guild {guildId}...");
            var projectSql = new QueryDefinition("SELECT * FROM c WHERE c.guildId = @guildId")
                .WithParameter("@guildId", guildId.ToString());

            var episodeSql = new QueryDefinition("SELECT * FROM c WHERE c.guildId = @guildId")
                .WithParameter("@guildId", guildId.ToString());

            // Get data
            List<Project> rawProjects = await AzureHelper.QueryProjects<Project>(projectSql);
            List<CachedEpisode> allEpisodes = await AzureHelper.QueryEpisodes<CachedEpisode>(episodeSql);

            // Transform data
            List<Project> cachedProjects = [];
            foreach (var project in rawProjects)
            {
                var episodes = allEpisodes.Where(e => e.ProjectId == project.Id).OrderBy(e => e.Number).ToList();
                _episodeCache[project.Id] = episodes;

                cachedProjects.Add(project);
            }
            _projectCache[guildId] = cachedProjects;
            log.Info($"Cache for guild {guildId} successfully rebuilt: {cachedProjects.Count} projects, {allEpisodes.Count} episodes");
        }

        /// <summary>
        /// Rebuild the project and episode cache for a specific project
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        public static async System.Threading.Tasks.Task RebuildCacheForProject(string projectId)
        {
            log.Info($"Rebuilding cache for project {projectId}...");

            var projectSql = new QueryDefinition("SELECT * FROM c WHERE c.id = @projectId")
                .WithParameter("@projectId", projectId.ToString());

            var episodeSql = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId")
                .WithParameter("@projectId", projectId.ToString());

            // Get data
            Project project = (await AzureHelper.QueryProjects<Project>(projectSql)).Single();
            List<CachedEpisode> episodes = (await AzureHelper.QueryEpisodes<CachedEpisode>(episodeSql)).OrderBy(e => e.Number).ToList();

            // Transform data
            var idx = _projectCache[project.GuildId].FindIndex(p => p.Id == project.Id);
            _projectCache[project.GuildId][idx] = project;

            _episodeCache[project.Id] = episodes;

            log.Info($"Cache for project {projectId} successfully rebuilt");
        }

        public static async System.Threading.Tasks.Task RebuildConfigCache()
        {
            log.Info($"Rebuilding config cache...");

            var configSql = new QueryDefinition("SELECT c.id, c.guildId, c.administratorIds FROM c");
            List<CachedConfig> configs = await AzureHelper.QueryConfigurations<CachedConfig>(configSql);

            _configCache.Clear();
            foreach (CachedConfig config in configs)
            {
                _configCache[config.GuildId] = config;
            }

            log.Info($"Config cache successfully rebuilt: {configs.Count} configurations");
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

            await RebuildConfigCache();

            log.Info($"Cache successfully built for {_projectCache.Count} guilds");
        }
    }

    /// <summary>
    /// Minimal config info structure for caching purposes
    /// </summary>
    internal record CachedConfig
    {
        public required string Id;
        public required ulong GuildId;
        public required ulong[] AdministratorIds;
    }

    /// <summary>
    /// Minimal episode structure for caching purposes
    /// </summary>
    internal record CachedEpisode
    {
        public required string Id;
        public required decimal Number;
        public required string ProjectId;
        public required Records.Task[] Tasks;
        public required bool Done;
        public required bool ReminderPosted;
    }
}
