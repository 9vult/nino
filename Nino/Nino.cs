using Discord;
using Discord.WebSocket;
using dotenv.net;
using Nino.Listeners;
using Nino.Utilities;
using NLog;
using System.Text;
using static Localizer.Localizer;

namespace Nino
{
    public class Nino
    {
        private static readonly DiscordSocketClient _client = new();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static DiscordSocketClient Client => _client;

        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Listener.SetupLogger();

            log.Info($"Starting Nino {Utils.VERSION}");

            // Read in environment variables
            var env = DotEnv.Read();
            if (!env.TryGetValue("AZURE_COSMOS_ENDPOINT", out var azureCosmosEndpoint)) throw new Exception("Missing env.AZURE_COSMOS_ENDPOINT!");
            if (!env.TryGetValue("AZURE_CLIENT_SECRET", out var azureClientSecret)) throw new Exception("Missing env.AZURE_CLIENT_SECRET!");
            if (!env.TryGetValue("AZURE_COSMOS_DB_NAME", out var azureCosmosName)) throw new Exception("Missing env.AZURE_COSMOS_DB_NAME!");
            if (!env.TryGetValue("DISCORD_API_TOKEN", out var discordApiToken)) throw new Exception("Missing env.DISCORD_API_TOKEN!");

            // Set up Azure database
            await AzureHelper.Setup(azureCosmosEndpoint, azureClientSecret, azureCosmosName);
            
            // Build initial cache
            await Cache.BuildCache();

            // Load localization files
            LoadStringLocalizations(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "i18n/str")));
            LoadCommandLocalizations(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "i18n/cmd")));
            

            // Listen up
            _client.Log += Listener.Log;
            _client.Ready += Listener.Ready;
            _client.SlashCommandExecuted += Listener.SlashCommandExecuted;
            _client.AutocompleteExecuted += Listener.AutocompleteExecuted;

            // Start the bot
            await _client.LoginAsync(TokenType.Bot, discordApiToken);
            await _client.StartAsync();
            await Task.Delay(-1);
        }
    }
}
