using System.Text;
using CommandLine;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Handlers;
using Nino.Services;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino
{
    public class Nino
    {
        private static CmdLineOptions _cmdLineOptions = new();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static AppConfig? _config;
        private static IServiceProvider? _services;
        private static DiscordSocketConfig _socketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged ^ GatewayIntents.GuildScheduledEvents ^ GatewayIntents.GuildInvites
        };

        public static DiscordSocketClient Client => _services!.GetRequiredService<DiscordSocketClient>();
        public static AppConfig Config => _config!;

        private static readonly InteractionServiceConfig _interactionServiceConfig = new()
        {
            LocalizationManager = new JsonLocalizationManager("i18n/cmd", "nino")
        };

        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            LogHandler.SetupLogger();

            log.Info($"Starting Nino {Utils.VERSION}");

            // Read in environment variables
            IConfigurationRoot configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            _config = configBuilder.GetRequiredSection("Configuration").Get<AppConfig?>();
            if (_config == null)
                throw new Exception("Missing appsettings.json!");

            // Read in command-line arguments
            _cmdLineOptions = Parser.Default.ParseArguments<CmdLineOptions>(args).Value;

            // Set up services
            _services = new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(_cmdLineOptions)
                .AddSingleton(_socketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), _interactionServiceConfig))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();

            // Set up Azure database
            await AzureHelper.Setup(_config.AzureCosmosEndpoint, _config.AzureClientSecret, _config.AzureCosmosDbName);

            // Build initial cache
            await Cache.BuildCache();

            // Start AniList service
            if (!_cmdLineOptions.DisableAniList)
                _ = new ReleaseReminderService();
            else
                AniListService.ANILIST_ENABLED = false;

            // Start Conga Reminder service
            _ = new CongaReminderService();

            // Load localization files
            LoadLocalizations(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "i18n/str")));

            // Set up client
            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogHandler.Log;

            await _services.GetRequiredService<InteractionHandler>().InitializeAsync();

            // Start the bot
            await client.LoginAsync(TokenType.Bot, _config.DiscordApiToken);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
