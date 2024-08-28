using Microsoft.Azure.Cosmos;
using Nino.Utilities;

namespace Nino
{
    internal static class AzureHelper
    {
        private static CosmosClient? _client;
        private static Database? _database;
        private static Container? _projectsContainer;
        private static Container? _episodesContainer;
        private static Container? _configurationContainer;
        private static Container? _observersContainer;

        public static Container? Projects => _projectsContainer;
        public static Container? Episodes => _episodesContainer;
        public static Container? Configurations => _configurationContainer;
        public static Container? Observers => _observersContainer;

        public static async Task Setup(string endpointUri, string primaryKey, string databaseName)
        {
            _client = new(
                endpointUri,
                primaryKey,
                new CosmosClientOptions
                {
                    ApplicationName = "Nino",
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }
                });

            _database = await _client.CreateDatabaseIfNotExistsAsync(databaseName);
            _projectsContainer = await _database.CreateContainerIfNotExistsAsync("Projects", "/guildId");
            _episodesContainer = await _database.CreateContainerIfNotExistsAsync("Episodes", "/projectId");
            _configurationContainer = await _database.CreateContainerIfNotExistsAsync("Configuration", "/guildId");
            _observersContainer = await _database.CreateContainerIfNotExistsAsync("Observers", "/originGuildId");
        }

        /// <summary>
        /// Query Configurations
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="sql">Query to run</param>
        /// <returns>List of resulting objects</returns>
        public static async Task<List<T>> QueryConfigurations<T>(QueryDefinition sql)
        {
            List<T> results = [];
            using FeedIterator<T> feed = Configurations!.GetItemQueryIterator<T>(queryDefinition: sql);
            while (feed.HasMoreResults)
            {
                FeedResponse<T> response = await feed.ReadNextAsync();
                foreach (T p in response)
                {
                    results.Add(p);
                }
            }
            return results;
        }

        /// <summary>
        /// Query Projects
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="sql">Query to run</param>
        /// <returns>List of resulting objects</returns>
        public static async Task<List<T>> QueryProjects<T>(QueryDefinition sql)
        {
            List<T> results = [];
            using FeedIterator<T> feed = Projects!.GetItemQueryIterator<T>(queryDefinition: sql);
            while (feed.HasMoreResults)
            {
                FeedResponse<T> response = await feed.ReadNextAsync();
                foreach (T p in response)
                {
                    results.Add(p);
                }
            }
            return results;
        }

        /// <summary>
        /// Query Episodes
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="sql">Query to run</param>
        /// <returns>List of resulting objects</returns>
        public static async Task<List<T>> QueryEpisodes<T>(QueryDefinition sql)
        {
            List<T> results = [];
            using FeedIterator<T> feed = Episodes!.GetItemQueryIterator<T>(queryDefinition: sql);
            while (feed.HasMoreResults)
            {
                FeedResponse<T> response = await feed.ReadNextAsync();
                foreach (T e in response)
                {
                    results.Add(e);
                }
            }
            return results;
        }

        /// <summary>
        /// Query Observers
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="sql">Query to run</param>
        /// <returns>List of resulting objects</returns>
        public static async Task<List<T>> QueryObservers<T>(QueryDefinition sql)
        {
            List<T> results = [];
            using FeedIterator<T> feed = Observers!.GetItemQueryIterator<T>(queryDefinition: sql);
            while (feed.HasMoreResults)
            {
                FeedResponse<T> response = await feed.ReadNextAsync();
                foreach (T e in response)
                {
                    results.Add(e);
                }
            }
            return results;
        }

        #region Partition Keys

        /// <summary>
        /// Partition Key for use when accessing Projects
        /// </summary>
        /// <param name="project">Project being accessed</param>
        /// <returns>Partition Key of the project's GuildId</returns>
        public static PartitionKey ProjectPartitionKey(Records.Project project)
        {
            return new PartitionKey(project.SerializationGuildId);
        }

        /// <summary>
        /// Partition Key for use when accessing Projects
        /// </summary>
        /// <param name="guildId">Guild ID for the project being accessed</param>
        /// <returns>Partition Key of the project's GuildId</returns>
        public static PartitionKey ProjectPartitionKey(ulong guildId)
        {
            return new PartitionKey(guildId.ToString());
        }

        /// <summary>
        /// Partition Key for use when accessing Episodes
        /// </summary>
        /// <param name="episode">Episode being accessed</param>
        /// <returns>Partition Key of the episode's ProjectId</returns>
        public static PartitionKey EpisodePartitionKey(Records.Episode episode)
        {
            return new PartitionKey(episode.ProjectId);
        }

        /// <summary>
        /// Partition Key for use when accessing Episodes
        /// </summary>
        /// <param name="episode">Episode being accessed</param>
        /// <returns>Partition Key of the episode's ProjectId</returns>
        public static PartitionKey EpisodePartitionKey(CachedEpisode episode)
        {
            return new PartitionKey(episode.ProjectId);
        }

        /// <summary>
        /// Partition Key for use when accessing Episodes
        /// </summary>
        /// <param name="project">Project the episode being accessed is from</param>
        /// <returns>Partition Key of the project's id</returns>
        public static PartitionKey EpisodePartitionKey(Records.Project project)
        {
            return new PartitionKey(project.Id);
        }

        /// <summary>
        /// Partition Key for use when accessing Configurations
        /// </summary>
        /// <param name="config">Configuration being accessed</param>
        /// <returns>Partition Key of the config's GuildId</returns>
        public static PartitionKey ConfigurationPartitionKey(Records.Configuration config)
        {
            return new PartitionKey(config.SerializationGuildId);
        }

        /// <summary>
        /// Partition Key for use when accessing Configurations
        /// </summary>
        /// <param name="originGuildId">Guild ID for the project being accessed</param>
        /// <returns>Partition Key of the configuration's GuildId</returns>
        public static PartitionKey ConfigurationPartitionKey(ulong guildId)
        {
            return new PartitionKey(originGuildId.ToString());
        }

        /// <summary>
        /// Partition Key for use when accessing Observers
        /// </summary>
        /// <param name="observer">Observer being accessed</param>
        /// <returns>Partition Key of the observer's OriginGuildId</returns>
        public static PartitionKey ObserverPartitionKey(Records.Observer observer)
        {
            return new PartitionKey(observer.SerializationOriginGuildId);
        }

        /// <summary>
        /// Partition Key for use when accessing Observers
        /// </summary>
        /// <param name="originGuildId">Origin Guild ID for the project being accessed</param>
        /// <returns>Partition Key of the observer's OriginGuildId</returns>
        public static PartitionKey ObserverPartitionKey(ulong originGuildId)
        {
            return new PartitionKey(originGuildId.ToString());
        }

#endregion Partition Keys
    }
}
