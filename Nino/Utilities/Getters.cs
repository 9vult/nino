using Microsoft.Azure.Cosmos;
using Nino.Records;
using NLog;

namespace Nino.Utilities
{
    internal static class Getters
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static List<string> GetFilteredAliases(ulong guildId, ulong userId, string query, bool includeObservers = false)
        {
            List<Project> projects = [];

            // Local guild projects
            projects.AddRange(Cache.GetProjects(guildId));
            
            // Observing guild projects
            if (includeObservers)
            {
                projects.AddRange(Cache.GetObservers()
                    .Where(o => o.GuildId == guildId)
                    .SelectMany(o => Cache.GetProjects().Where(p => p.Id == o.ProjectId)));
            }

            // Local guild admins
            var guildAdmins = Cache.GetConfig(guildId)?.AdministratorIds ?? [];

            // Filter
            var filtered = projects.Where(
                p => !p.IsPrivate || 
                p.OwnerId == userId || 
                p.AdministratorIds.Any(a => a == userId) ||
                guildAdmins.Any(a => a == userId) ||
                p.KeyStaff.Any(ks => ks.UserId == userId)).ToList();
            // Not including Additional Staff here because that'd be a royal pita to get

            return filtered.SelectMany(p => new[] { p.Nickname }.Concat(p.Aliases))
                .Where(a => a.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public static async Task<List<Episode>> GetEpisodes(Project project)
        {
            var sql = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId")
                .WithParameter("@projectId", project.Id);

            List<Episode> results = [];

            using FeedIterator<Episode> feed = AzureHelper.Episodes!.GetItemQueryIterator<Episode>(queryDefinition: sql);
            while (feed.HasMoreResults)
            {
                FeedResponse<Episode> response = await feed.ReadNextAsync();
                foreach (Episode e in response)
                {
                    results.Add(e);
                }
            }
            
            return results;
        }

        public static async Task<Episode?> GetEpisode(Project project, decimal number)
        {
            var id = $"{project.Id}-{number}";
            
            try
            {
                var response = await AzureHelper.Episodes!.ReadItemAsync<Episode>(id: id, partitionKey: AzureHelper.EpisodePartitionKey(project));
                return response?.Resource;
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (CosmosException e)
            {
                log.Error(e.Message);
                return null;
            }
        }

        public static async Task<Project?> GetProject(string projectId, ulong guildId)
        {
            try
            {
                var response = await AzureHelper.Projects!.ReadItemAsync<Project>(id: projectId, partitionKey: AzureHelper.ProjectPartitionKey(guildId));
                return response?.Resource;
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (CosmosException e)
            {
                log.Error(e.Message);
                return null;
            }
        }

        public static async Task<Configuration?> GetConfiguration(ulong guildId)
        {
            try
            {
                var response = await AzureHelper.Configurations!.ReadItemAsync<Configuration>(id: $"{guildId}-conf", partitionKey: AzureHelper.ConfigurationPartitionKey(guildId));
                return response?.Resource;
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (CosmosException e)
            {
                log.Error(e.Message);
                return null;
            }
        }
    }
}
