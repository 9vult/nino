using Microsoft.Azure.Cosmos;
using Nino.Records;
using NLog;
using System.Diagnostics.CodeAnalysis;

namespace Nino.Utilities
{
    internal static class Getters
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static List<string> GetFilteredAliases(ulong guildId, ulong userId, string query, bool includeObservers = false, bool includeArchived = false)
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
            
            if (!includeArchived)
                projects = projects.Where(p => !p.IsArchived).ToList();

            // Local guild admins
            var guildAdmins = Cache.GetConfig(guildId)?.AdministratorIds ?? [];

            // Filter (Not including Additional Staff here because that'd be a royal pita to get)
            return projects.Where(p => !p.IsPrivate ||
                                       p.OwnerId == userId ||
                                       p.AdministratorIds.Any(a => a == userId) ||
                                       guildAdmins.Any(a => a == userId) ||
                                       p.KeyStaff.Any(ks => ks.UserId == userId))
                .SelectMany(p => new[] { p.Nickname }.Concat(p.Aliases))
                .Where(a => a.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        /// <summary>
        /// Try to get an episode
        /// </summary>
        /// <param name="project">Project to get the episode from</param>
        /// <param name="number">Number of the episode</param>
        /// <param name="episode">Episode</param>
        /// <returns>Found episode, or <see langword="null"/> if it doesn't exist</returns>
        public static bool TryGetEpisode (Project project, string number, [MaybeNullWhen(false)] out Episode episode)
        {
            episode = Cache.GetEpisodes(project.Id).FirstOrDefault(e => e.Number == number);
            return episode is not null;
        }
        
        private static async Task<Episode?> GetEpisode(Project project, string number)
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
