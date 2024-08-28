using System;

namespace Nino
{
    public class AppConfig
    {
        public required string AzureCosmosEndpoint { get; set; }
        public required string AzureClientSecret { get; set; }
        public required string AzureCosmosDbName { get; set; }
        public required string DiscordApiToken { get; set; }
        public required string AniDbApiClientName { get; set; }
    }
}
