// SPDX-License-Identifier: MPL-2.0

using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;

namespace Nino.Discord.Handlers;

public class InteractionHandler(
    IServiceProvider provider,
    DiscordSocketClient client,
    InteractionService handler,
    ILogger<InteractionHandler> logger
)
{
    public async Task InitializeAsync()
    {
        client.Ready += OnClientReadyAsync;
        client.SlashCommandExecuted += OnSlashCommandInteraction;
        client.ButtonExecuted += OnButtonClickedInteraction;

        await handler.AddModulesAsync(Assembly.GetExecutingAssembly(), provider);
    }

    private async Task OnClientReadyAsync()
    {
        await handler.RegisterCommandsGloballyAsync();
    }

    private async Task OnSlashCommandInteraction(SocketSlashCommand interaction)
    {
        try
        {
            if (interaction.GuildId is null)
            {
                await interaction.RespondAsync("Nino commands must be run in a server!");
                return;
            }

            await interaction.DeferAsync();

            var context = new SocketInteractionContext(client, interaction);
            await handler.ExecuteCommandAsync(context, provider);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while executing a command.");
        }
    }

    private async Task OnButtonClickedInteraction(SocketMessageComponent interaction)
    {
        try
        {
            if (interaction.GuildId is null)
            {
                await interaction.RespondAsync("Nino commands must be run in a server!");
                return;
            }

            await interaction.DeferAsync();

            var context = new SocketInteractionContext(client, interaction);
            await handler.ExecuteCommandAsync(context, provider);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while executing a button.");
        }
    }
}
