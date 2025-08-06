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
using System.Globalization;
using static Localizer.Localizer;

namespace Nino
{
    public class Nino
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static CmdLineOptions _cmdLineOptions = new();
        private static AppConfig? _config;
        private static IServiceProvider? _services;
        
        private static readonly DiscordSocketConfig SocketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged ^ GatewayIntents.GuildScheduledEvents ^ GatewayIntents.GuildInvites
        };

        public static DiscordSocketClient Client => _services!.GetRequiredService<DiscordSocketClient>();
        public static AppConfig Config => _config!;
        public static DataContext DataContext => _services!.GetRequiredService<DataContext>();

        private static readonly InteractionServiceConfig InteractionServiceConfig = new()
        {
            LocalizationManager = new JsonLocalizationManager("i18n/cmd", "nino")
        };

        public static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            
            Console.OutputEncoding = Encoding.UTF8;
            LogHandler.SetupLogger();

            Log.Info($"Starting Nino {Utils.Version}");

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
                .AddDbContext<DataContext>()
                .AddSingleton(_config)
                .AddSingleton(_cmdLineOptions)
                .AddSingleton(SocketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), InteractionServiceConfig))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<ReleaseReminderService>()
                .AddSingleton<CongaReminderService>()
                .BuildServiceProvider();

            // Start AniList service
            if (!_cmdLineOptions.DisableAniList)
                _ = _services.GetRequiredService<ReleaseReminderService>();
            else
                AniListService.AniListEnabled = false;

            // Start Conga Reminder service
            _ = _services.GetRequiredService<CongaReminderService>();

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
