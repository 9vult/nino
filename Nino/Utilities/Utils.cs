using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Utilities
{
    internal static class Utils
    {
        public static async Task<Project?> ResolveAlias(string query, SocketSlashCommand interaction, ulong? observingGuildId = null)
        {
            var guildId = observingGuildId ?? interaction.GuildId;

            var sql = new QueryDefinition("SELECT * from c WHERE c.guildId = @guildId AND c.nickname = @query OR ARRAY_CONTAINS(c.aliases, @query)")
                .WithParameter("@query", query)
                .WithParameter("@guildId", guildId.ToString());

            using FeedIterator<Project> feed = AzureHelper.Projects!.GetItemQueryIterator<Project>(queryDefinition: sql);
            if (feed.HasMoreResults)
            {
                FeedResponse<Project> response = await feed.ReadNextAsync();
                return response.FirstOrDefault();
            }
            // No matches
            return null;
        }

        public static bool VerifyUser(ulong userId, Project project, bool excludeAdmins = false)
        {
            if (project.OwnerId == userId) return true;

            if (!excludeAdmins)
            {
                if (project.AdministratorIds.Any(a => a == userId))
                    return true;
            }

            return false;
        }
    }
}
