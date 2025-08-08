using System.Globalization;
using System.Text;
using CommandLine;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.EntityFrameworkCore;
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
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static CmdLineOptions _cmdLineOptions = new();
        private static IServiceProvider? _services;

        private static readonly DiscordSocketConfig SocketConfig = new()
        {
            GatewayIntents =
                GatewayIntents.AllUnprivileged
                ^ GatewayIntents.GuildScheduledEvents
                ^ GatewayIntents.GuildInvites,
        };

        public static DiscordSocketClient Client =>
            _services!.GetRequiredService<DiscordSocketClient>();
        public static AppConfig Config { get; private set; } = null!;

        private static readonly InteractionServiceConfig InteractionServiceConfig = new()
        {
            LocalizationManager = new JsonLocalizationManager("i18n/cmd", "nino"),
        };

        private static void ConfigureServices()
        {
            _services = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddDbContext<DataContext>()
                .AddSingleton(Config)
                .AddSingleton(_cmdLineOptions)
                .AddSingleton(SocketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(
                    x.GetRequiredService<DiscordSocketClient>(),
                    InteractionServiceConfig
                ))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<ReleaseReminderService>()
                .AddSingleton<CongaReminderService>()
                .AddSingleton<ObserverPublisher>()
                .BuildServiceProvider();
        }

        private static async Task InitializeDatabase()
        {
            var db = _services!.GetRequiredService<DataContext>();
            await db.Database.MigrateAsync();

            var projectCount = db.Projects.Count();
            var episodeCount = db.Episodes.Count();
            var guildCount = db.Projects.GroupBy(p => p.GuildId).Count();
            
            Log.Info(
                $"Database initialized with {projectCount} projects ({episodeCount} episodes) from {guildCount} guilds"
            );
        }

        private static async Task InitializeDiscordClient()
        {
            var client = _services!.GetRequiredService<DiscordSocketClient>();
            client.Log += LogHandler.Log;
            await _services!.GetRequiredService<InteractionHandler>().InitializeAsync();
            await client.LoginAsync(TokenType.Bot, Config.DiscordApiToken);
            await client.StartAsync();
        }

        public static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Console.OutputEncoding = Encoding.UTF8;
            LogHandler.SetupLogger();
            Log.Info($"Starting Nino {Utils.Version}");

            // Read in environment variables and cmd-line options
            var configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            Config =
                configBuilder.GetRequiredSection("Configuration").Get<AppConfig?>()
                ?? throw new Exception("Missing appsettings.json!");
            
            _cmdLineOptions = Parser.Default.ParseArguments<CmdLineOptions>(args).Value;
            AniListService.AniListEnabled = !_cmdLineOptions.DisableAniList;

            // Configure DI
            ConfigureServices();

            // Start required background services
            if (!AniListService.AniListEnabled)
                _ = _services!.GetRequiredService<ReleaseReminderService>();
            _ = _services!.GetRequiredService<CongaReminderService>();

            // Load localization files
            LoadLocalizations(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "i18n/str")));
            
            // Initialize database
            await InitializeDatabase();

            // Initialize Discord client
            await InitializeDiscordClient();

            await Task.Delay(-1);
        }
    }
}
