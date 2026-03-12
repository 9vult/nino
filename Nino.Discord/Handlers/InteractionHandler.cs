// SPDX-License-Identifier: MPL-2.0

using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nino.Discord.Handlers;

public sealed class InteractionHandler(
    IServiceProvider provider,
    DiscordSocketClient client,
    InteractionService handler,
    IOptions<DiscordOptions> options,
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
        if (options.Value.GuildId is null)
        {
            logger.LogInformation("Registering commands globally...");
            await handler.RegisterCommandsGloballyAsync();
        }
        else
        {
            var guildId = options.Value.GuildId.Value;
            logger.LogInformation("Registering commands to guild {GuildId}...", guildId);
            await handler.RegisterCommandsToGuildAsync(guildId);
        }
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

            using var scope = logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = Guid.NewGuid(),
                    ["InteractionType"] = "SlashCommand",
                    ["CommandName"] = interaction.CommandName,
                }
            );

            // Auto-defer
            await interaction.DeferAsync();

            var context = new SocketInteractionContext(client, interaction);
            await handler.ExecuteCommandAsync(context, provider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing a slash command.");
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

            var buttonName = interaction.Data.CustomId[..interaction.Data.CustomId.IndexOf(':')];

            using var scope = logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = Guid.NewGuid(),
                    ["InteractionType"] = "SlashCommand",
                    ["CommandName"] = buttonName,
                }
            );

            // Auto-defer
            await interaction.DeferAsync();

            var context = new SocketInteractionContext(client, interaction);
            await handler.ExecuteCommandAsync(context, provider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing a button command.");
        }
    }
}
