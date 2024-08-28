﻿using Discord;
using Discord.WebSocket;
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
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static AppConfig? _config;

        public static DiscordSocketClient Client => _client;
        public static AppConfig Config => _config!;

        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Listener.SetupLogger();

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
