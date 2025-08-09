using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NLog;

using static Localizer.Localizer;

namespace Nino.Handlers
{
    public class InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, CmdLineOptions cmdLineOptions)
    {
        private readonly DiscordSocketClient _client = client;
        private readonly InteractionService _handler = handler;
        private readonly IServiceProvider _services = services;
        private readonly CmdLineOptions _cmdLineOptions = cmdLineOptions;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
#if DEBUG
            Log.Info("Running in debug mode. Deploying slash commands...");
            await _handler.RegisterCommandsGloballyAsync();
            Log.Info("Slash commands deployed");
#else
            if (_cmdLineOptions.DeployCommands)
            {
                Log.Info("--deploy-commands is set. Deploying slash commands...");
                await _handler.RegisterCommandsGloballyAsync();
                Log.Info("Slash commands deployed");
            }
#endif
        }

        private async Task HandleSlashCommandInteraction(SocketSlashCommand interaction)
        {
            try
            {
                var guildId = interaction.GuildId;
                if (guildId == null)
                {
                    await interaction.RespondAsync("Nino commands must be run in a server!");
                    return;
                }

                await interaction.DeferAsync();

                var context = new SocketInteractionContext(_client, interaction);
                await _handler.ExecuteCommandAsync(context, _services);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                await interaction.FollowupAsync(T("error.generic", interaction.UserLocale));
            }
        }

        private async Task HandleAutocompleteInteraction(SocketAutocompleteInteraction interaction)
        {
            var context = new InteractionContext(_client, interaction, interaction.Channel);
            await _handler.ExecuteCommandAsync(context, _services);
        }
    }
}
