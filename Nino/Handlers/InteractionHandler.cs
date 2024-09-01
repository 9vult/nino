using System;
using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using NLog;

namespace Nino.Handlers
{
    public class InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, CmdLineOptions cmdLineOptions)
    {
        private readonly DiscordSocketClient _client = client;
        private readonly InteractionService _handler = handler;
        private readonly IServiceProvider _services = services;
        private readonly CmdLineOptions _cmdLineOptions = cmdLineOptions;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public async Task InitializeAsync()
        {
            _client.Ready += ReadyAsync;
            _handler.Log += LogHandler.Log;

            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.SlashCommandExecuted += HandleSlashCommandInteraction;
            _client.AutocompleteExecuted += HandleAutocompleteInteraction;
        }

        private async Task ReadyAsync()
        {
            if (_cmdLineOptions.DeployCommands || true) // TODO: Disable
            {
                log.Info("--deploy-commands is set. Deploying slash commands...");
                await _handler.RegisterCommandsGloballyAsync();
                log.Info("Slash commands deployed");
            }
        }

        private async Task HandleSlashCommandInteraction(SocketSlashCommand interaction)
        {
            try
            {
                var guildId = interaction.GuildId;
                if (guildId == null)
                {
                    await interaction.FollowupAsync("Nino commands must be run in a server!");
                    return;
                }

                await interaction.DeferAsync();

                var context = new SocketInteractionContext(_client, interaction);
                await _handler.ExecuteCommandAsync(context, _services);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        private async Task HandleAutocompleteInteraction(SocketAutocompleteInteraction interaction)
        {
            var context = new InteractionContext(_client, interaction, interaction.Channel);
            await _handler.ExecuteCommandAsync(context, _services);
        }
    }
}
