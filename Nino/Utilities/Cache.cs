using Microsoft.Azure.Cosmos;
using NaturalSort.Extension;
using Nino.Records;
using NLog;

namespace Nino.Utilities
{
    internal static class Cache
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<ulong, List<Project>> ProjectCache = [];
        private static readonly Dictionary<ulong, Configuration> ConfigCache = [];
        private static readonly Dictionary<Guid, List<Episode>> EpisodeCache = [];
        private static readonly Dictionary<ulong, List<Observer>> ObserverCache = [];

        /// <summary>
        /// Rebuild the project and episode cache for a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        public static async System.Threading.Tasks.Task RebuildCacheForGuild(ulong guildId)
        {
            Log.Info($"Rebuilding cache for guild {guildId}...");
            
            // Remove outdated data
            foreach (var project in ProjectCache[guildId])
                EpisodeCache.Remove(project.Id);
            ProjectCache.Remove(guildId);
            
            var projectSql = new QueryDefinition(
                "SELECT * FROM c WHERE c.guildId = @guildId"
            ).WithParameter("@guildId", guildId.ToString());

            var episodeSql = new QueryDefinition(
                "SELECT * FROM c WHERE c.guildId = @guildId"
            ).WithParameter("@guildId", guildId.ToString());

            // Get new data
            var rawProjects = await AzureHelper.QueryProjects<Project>(projectSql);
            var allEpisodes = await AzureHelper.QueryEpisodes<Episode>(episodeSql);

            // Transform data
            List<Project> cachedProjects = [];
            foreach (var project in rawProjects)
            {
                var episodes = allEpisodes
                    .Where(e => e.ProjectId == project.Id)
                    .OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
                    .ToList();
                EpisodeCache[project.Id] = episodes;

                cachedProjects.Add(project);
            }
            ProjectCache[guildId] = cachedProjects;
            Log.Info(
                $"Cache for guild {guildId} successfully rebuilt: {cachedProjects.Count} projects, {allEpisodes.Count} episodes"
            );
        }

        /// <summary>
        /// Rebuild the project and episode cache for a specific project
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        public static async System.Threading.Tasks.Task RebuildCacheForProject(Guid projectId)
        {
            Log.Info($"Rebuilding cache for project {projectId}...");

            var projectSql = new QueryDefinition(
                "SELECT * FROM c WHERE c.id = @projectId"
            ).WithParameter("@projectId", projectId.ToString());

            var episodeSql = new QueryDefinition(
                "SELECT * FROM c WHERE c.projectId = @projectId"
            ).WithParameter("@projectId", projectId.ToString());

            // Get data
            var project = (await AzureHelper.QueryProjects<Project>(projectSql)).Single();
            var episodes = (await AzureHelper.QueryEpisodes<Episode>(episodeSql))
                .OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
                .ToList();

            // Transform data
            var idx = ProjectCache[project.GuildId].FindIndex(p => p.Id == project.Id);
            ProjectCache[project.GuildId][idx] = project;

            EpisodeCache[project.Id] = episodes;

            Log.Info($"Cache for project {projectId} successfully rebuilt");
        }

        public static async System.Threading.Tasks.Task RebuildConfigCache()
        {
            Log.Info($"Rebuilding config cache...");

            var configSql = new QueryDefinition("SELECT * FROM c");
            var configs = await AzureHelper.QueryConfigurations<Configuration>(configSql);

            ConfigCache.Clear();
            foreach (var config in configs)
            {
                ConfigCache[config.GuildId] = config;
            }

            Log.Info($"Config cache successfully rebuilt: {configs.Count} configurations");
        }

        public static async System.Threading.Tasks.Task RebuildObserverCache()
        {
            Log.Info($"Rebuilding observer cache...");

            var configSql = new QueryDefinition("SELECT * FROM c");
            var allObservers = await AzureHelper.QueryObservers<Observer>(configSql);

            ObserverCache.Clear();
            foreach (var observer in allObservers)
            {
                if (!ObserverCache.TryGetValue(observer.OriginGuildId, out var guildObservers))
                {
                    guildObservers = [];
                    ObserverCache[observer.OriginGuildId] = guildObservers;
                }
                guildObservers.Add(observer);
            }

            Log.Info($"Observer cache successfully rebuilt: {allObservers.Count} observers");
        }

        /// <summary>
        /// Build the cache
        /// </summary>
        public static async System.Threading.Tasks.Task BuildCache()
        {
            Log.Info("Building cache...");
            ProjectCache.Clear();
            EpisodeCache.Clear();
            ConfigCache.Clear();
            ObserverCache.Clear();
            var sql = new QueryDefinition("SELECT DISTINCT c.guildId FROM c");
            var response = await AzureHelper.QueryProjects<dynamic>(sql);

            foreach (var guildId in response.Select(item => ulong.Parse((string)item.guildId)))
            {
                await RebuildCacheForGuild(guildId);
            }

            await RebuildConfigCache();
            await RebuildObserverCache();

            Log.Info($"Cache successfully built for {ProjectCache.Count} guilds");
        }

        #region getters

        /// <summary>
        /// Get a list of project guilds
        /// </summary>
        /// <returns>List of guilds with projects</returns>
        public static List<ulong> GetProjectGuilds()
        {
            return ProjectCache.Keys.ToList();
        }

        /// <summary>
        /// Get a cached config
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <returns>Cached config</returns>
        public static Configuration? GetConfig(ulong guildId)
        {
            return ConfigCache.GetValueOrDefault(guildId);
        }

        /// <summary>
        /// Get a flattened list of all cached projects
        /// </summary>
        /// <returns>List of all projects</returns>
        public static List<Project> GetProjects()
        {
            return ProjectCache.Values.SelectMany(list => list).ToList();
        }

        /// <summary>
        /// Get the cached projects for a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <returns>The projects, or an empty list</returns>
        public static List<Project> GetProjects(ulong guildId)
        {
            return ProjectCache.TryGetValue(guildId, out var projectCache) ? projectCache : [];
        }

        /// <summary>
        /// Get a flattened list of all cached episodes
        /// </summary>
        /// <returns>List of all episodes</returns>
        public static List<Episode> GetEpisodes()
        {
            return EpisodeCache.Values.SelectMany(list => list).ToList();
        }

        /// <summary>
        /// Get the cached episodes for a project
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <returns>The episodes, or an empty list</returns>
        public static List<Episode> GetEpisodes(Guid projectId)
        {
            return EpisodeCache.TryGetValue(projectId, out var episodeCache) ? episodeCache : [];
        }

        /// <summary>
        /// Get cached observers for an origin guild
        /// </summary>
        /// <param name="guildId">ID of the observers</param>
        /// <returns>Cached observer</returns>
        public static List<Observer> GetObservers(ulong guildId)
        {
            return ObserverCache.TryGetValue(guildId, out var cachedObservers)
                ? cachedObservers
                : [];
        }

        /// <summary>
        /// Get a flattened list of all cached observers
        /// </summary>
        /// <returns>List of all observers</returns>
        public static List<Observer> GetObservers()
        {
            return ObserverCache.Values.SelectMany(list => list).ToList();
        }

        #endregion getters
    }
}
