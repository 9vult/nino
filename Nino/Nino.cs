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
using Task = System.Threading.Tasks.Task;

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

        public static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Console.OutputEncoding = Encoding.UTF8;
            LogHandler.SetupLogger();
            Log.Info($"Starting Nino Migration Utility {Utils.Version}");

            // Configure DI
            ConfigureServices();

            // Initialize database
            await InitializeDatabase();
            
            var m = new Migrator(_services!.GetRequiredService<DataContext>());
            await m.Migrate();

            await Task.Delay(-1);
        }
    }
}
