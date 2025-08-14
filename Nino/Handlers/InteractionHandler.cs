using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using NLog;
using static Localizer.Localizer;

namespace Nino.Handlers
{
    public class InteractionHandler(
        DiscordSocketClient client,
        InteractionService handler,
        IServiceProvider services,
        CmdLineOptions cmdLineOptions
    )
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public async Task InitializeAsync()
        {
            client.Ready += ReadyAsync;
            handler.Log += LogHandler.Log;

            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await handler.AddModulesAsync(Assembly.GetEntryAssembly(), services);

            // Process the InteractionCreated payloads to execute Interactions commands
            client.SlashCommandExecuted += HandleSlashCommandInteraction;
            client.AutocompleteExecuted += HandleAutocompleteInteraction;
        }

        private async Task ReadyAsync()
        {
#if DEBUG
            Log.Info("Running in debug mode. Deploying slash commands...");
            await handler.RegisterCommandsGloballyAsync();
            Log.Info("Slash commands deployed");
#else
            if (cmdLineOptions.DeployCommands)
            {
                Log.Info("--deploy-commands is set. Deploying slash commands...");
                await handler.RegisterCommandsGloballyAsync();
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

                var context = new SocketInteractionContext(client, interaction);
                await handler.ExecuteCommandAsync(context, services);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                await interaction.FollowupAsync(T("error.generic", interaction.UserLocale));
            }
        }

        private async Task HandleAutocompleteInteraction(SocketAutocompleteInteraction interaction)
        {
            var context = new InteractionContext(client, interaction, interaction.Channel);
            await handler.ExecuteCommandAsync(context, services);
        }
    }
}
