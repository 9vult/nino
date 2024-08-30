using System;
using CommandLine;

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

    public class CmdLineOptions
    {
        [Option("d", "deploy-commands", Required = false, HelpText = "(Re)deploy slash commands on startup")]
        public bool DeployCommands { get; set; }
    }
}
