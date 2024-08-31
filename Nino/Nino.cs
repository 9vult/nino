using CommandLine;
using Discord;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Configuration;
using Nino.Listeners;
using Nino.Services;
using Nino.Utilities;
using NLog;
using System.Text;
using static Localizer.Localizer;

namespace Nino
{
    public class Nino
    {
        private static readonly DiscordSocketClient _client = new();
        private static readonly CmdLineOptions _cmdLineOptions = new();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly InteractiveService _interactiveService = new(_client);
        private static AppConfig? _config;

        public static DiscordSocketClient Client => _client;
        public static InteractiveService InteractiveService => _interactiveService;
        public static AppConfig Config => _config!;
        public static CmdLineOptions CmdLineOptions => _cmdLineOptions;

        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Listener.SetupLogger();

            // Read in command-line arguments
            CommandLineParser.Default.ParseArguments(args, _cmdLineOptions);

            log.Info($"Starting Nino {Utils.VERSION}");

            // Read in environment variables
            IConfigurationRoot configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            _config = configBuilder.GetRequiredSection("Configuration").Get<AppConfig?>();
            if (_config == null)
                throw new Exception("Missing appsettings.json!");
            
            // Set up Azure database
            await AzureHelper.Setup(_config.AzureCosmosEndpoint, _config.AzureClientSecret, _config.AzureCosmosDbName);
            
            // Build initial cache
            await Cache.BuildCache();

            // Start services
            var reminderService = new ReleaseReminderService();

            // Load localization files
            LoadStringLocalizations(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "i18n/str")));
            LoadCommandLocalizations(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "i18n/cmd")));  

            CmdLineOptions.DeployCommands = false;          

            // Listen up
            _client.Log += Listener.Log;
            _client.Ready += Listener.Ready;
            _client.SlashCommandExecuted += Listener.SlashCommandExecuted;
            _client.AutocompleteExecuted += Listener.AutocompleteExecuted;

            // Start the bot
            await _client.LoginAsync(TokenType.Bot, _config.DiscordApiToken);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
