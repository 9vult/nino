using CommandLine;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Handlers;
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
        private static IServiceProvider _services;

        public static DiscordSocketClient Client => _client;
        public static InteractiveService InteractiveService => _interactiveService;
        public static AppConfig Config => _config!;
        public static CmdLineOptions CmdLineOptions => _cmdLineOptions;

        private static readonly InteractionServiceConfig _interactionServiceConfig = new()
        {
            // LocalizationManager = new ResxLocalizationManager("InteractionFramework.Resources.CommandLocales", Assembly.GetEntryAssembly(),
            //         new CultureInfo("en-US"), new CultureInfo("ru"))
        };

        private static IServiceProvider CreateProvider()
        {
            var collection = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), _interactionServiceConfig))
                .AddSingleton<InteractionHandler>();

            return collection.BuildServiceProvider();
        }

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

            // Set up client
            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += Listener.Log;

            client.InteractionCreated += async (x) =>
            {
                var ctx = new SocketInteractionContext(client, x);
                await _services.GetRequiredService<InteractionService>().ExecuteCommandAsync(ctx, _services);
            };

            await _services.GetRequiredService<InteractionHandler>().InitializeAsync();

            // Start the bot
            await client.LoginAsync(TokenType.Bot, _config.DiscordApiToken);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
