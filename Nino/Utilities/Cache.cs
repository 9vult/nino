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
        private static readonly Dictionary<ulong, Configuration> _configCache = [];
        private static readonly Dictionary<string, List<Episode>> _episodeCache = [];
        private static readonly Dictionary<ulong, List<Observer>> _observerCache = [];

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
            List<Episode> allEpisodes = await AzureHelper.QueryEpisodes<Episode>(episodeSql);

            // Transform data
            List<Project> cachedProjects = [];
            foreach (var project in rawProjects)
            {
                var episodes = allEpisodes.Where(e => e.ProjectId == project.Id).OrderBy(e => e.Number, new NumericalStringComparer()).ToList();
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
            List<Episode> episodes = (await AzureHelper.QueryEpisodes<Episode>(episodeSql)).OrderBy(e => e.Number, new NumericalStringComparer()).ToList();

            // Transform data
            var idx = _projectCache[project.GuildId].FindIndex(p => p.Id == project.Id);
            _projectCache[project.GuildId][idx] = project;

            _episodeCache[project.Id] = episodes;

            log.Info($"Cache for project {projectId} successfully rebuilt");
        }

        public static async System.Threading.Tasks.Task RebuildConfigCache()
        {
            log.Info($"Rebuilding config cache...");

            var configSql = new QueryDefinition("SELECT c.id, c.guildId, c.releasePrefix, c.administratorIds FROM c");
            List<Configuration> configs = await AzureHelper.QueryConfigurations<Configuration>(configSql);

            _configCache.Clear();
            foreach (var config in configs)
            {
                _configCache[config.GuildId] = config;
            }

            log.Info($"Config cache successfully rebuilt: {configs.Count} configurations");
        }

        public static async System.Threading.Tasks.Task RebuildObserverCache()
        {
            log.Info($"Rebuilding observer cache...");

            var configSql = new QueryDefinition("SELECT * FROM c");
            List<Observer> allObservers = await AzureHelper.QueryObservers<Observer>(configSql);

            _observerCache.Clear();
            foreach (var observer in allObservers)
            {
                if (!_observerCache.TryGetValue(observer.OriginGuildId, out var guildObservers))
                {
                    guildObservers = [];
                    _observerCache[observer.OriginGuildId] = guildObservers;
                }
                guildObservers.Add(observer);
            }

            log.Info($"Observer cache successfully rebuilt: {allObservers.Count} observers");
        }

        /// <summary>
        /// Build the cache
        /// </summary>
        public static async System.Threading.Tasks.Task BuildCache()
        {
            log.Info("Building cache...");
            _projectCache.Clear();
            _episodeCache.Clear();
            _configCache.Clear();
            _observerCache.Clear();
            var sql = new QueryDefinition("SELECT DISTINCT c.guildId FROM c");
            var response = await AzureHelper.QueryProjects<dynamic>(sql);

            foreach (var item in response)
            {
                var guildId = ulong.Parse((string)item.guildId);
                await RebuildCacheForGuild(guildId);
            }

            await RebuildConfigCache();
            await RebuildObserverCache();

            log.Info($"Cache successfully built for {_projectCache.Count} guilds");
        }

        #region getters

        /// <summary>
        /// Get a cached config
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <returns>Cached config</returns>
        public static Configuration? GetConfig(ulong guildId)
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
        public static List<Episode> GetEpisodes()
        {
            return _episodeCache.Values.SelectMany(list => list).ToList();
        }

        /// <summary>
        /// Get the cached episodes for a project
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <returns>The episodes, or an empty list</returns>
        public static List<Episode> GetEpisodes(string projectId)
        {
            if (_episodeCache.TryGetValue(projectId, out var episodeCache))
                return episodeCache;
            return [];
        }

        /// <summary>
        /// Get cached observers for an origin guild
        /// </summary>
        /// <param name="guildId">ID of the observers</param>
        /// <returns>Cached observer</returns>
        public static List<Observer> GetObservers(ulong guildId)
        {
            if (_observerCache.TryGetValue(guildId, out var cachedObservers))
                return cachedObservers;
            return [];
        }

        /// <summary>
        /// Get a flattened list of all cached observers
        /// </summary>
        /// <returns>List of all observers</returns>
        public static List<Observer> GetObservers()
        {
            return _observerCache.Values.SelectMany(list => list).ToList();
        }

        #endregion getters
    }
}
