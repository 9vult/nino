using Microsoft.Azure.Cosmos;
using Microsoft.VisualBasic;
using Nino.Records;
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
    }
}
