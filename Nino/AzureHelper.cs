using Azure.Identity;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino
{
    internal static class AzureHelper
    {
        private static CosmosClient? _client;
        private static Database? _database;
        private static Container? _projectsContainer;
        private static Container? _episodesContainer;

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
        }
    }
}
