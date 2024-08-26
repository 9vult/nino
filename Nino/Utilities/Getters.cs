using Microsoft.Azure.Cosmos;
using Microsoft.VisualBasic;
using Nino.Records;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Utilities
{
    internal static class Getters
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

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
    }
}
