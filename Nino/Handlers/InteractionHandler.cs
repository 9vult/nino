using System;
using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Nino.Listeners;
using NLog;

namespace Nino.Handlers
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, IConfiguration config)
        {
            _client = client;
            _handler = handler;
            _services = services;
            _configuration = config;
        }

        public async Task InitializeAsync()
        {
            _client.Ready += ReadyAsync;
            _handler.Log += Listener.Log;

            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;
        }

        private async Task ReadyAsync()
        {
            await _handler.RegisterCommandsGloballyAsync();
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _handler.ExecuteCommandAsync(context, _services);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }
    }
}
