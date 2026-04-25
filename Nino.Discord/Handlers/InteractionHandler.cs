// SPDX-License-Identifier: MPL-2.0

using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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
        client.AutocompleteExecuted += OnAutocompleteInteraction;

        handler.InteractionExecuted += OnInteractionExecuted;

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
        if (interaction.GuildId is null)
        {
            await interaction.RespondAsync("Nino commands must be run in a server!");
            return;
        }

        if (options.Value.MaintenanceGate && interaction.User.Id != options.Value.OwnerId)
        {
            await interaction.RespondAsync(
                embed: new EmbedBuilder()
                    .WithTitle(T("nino.maintenance.title", interaction.UserLocale))
                    .WithDescription(T("nino.maintenance.body", interaction.UserLocale))
                    .WithThumbnailUrl("https://files.catbox.moe/j3qizm.png")
                    .Build(),
                ephemeral: false
            );
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

    private async Task OnButtonClickedInteraction(SocketMessageComponent interaction)
    {
        if (interaction.GuildId is null)
        {
            await interaction.RespondAsync("Nino commands must be run in a server!");
            return;
        }

        if (options.Value.MaintenanceGate && interaction.User.Id != options.Value.OwnerId)
        {
            await interaction.RespondAsync(
                embed: new EmbedBuilder()
                    .WithTitle(T("nino.maintenance.title", interaction.UserLocale))
                    .WithDescription(T("nino.maintenance.body", interaction.UserLocale))
                    .WithThumbnailUrl("https://files.catbox.moe/j3qizm.png")
                    .Build(),
                ephemeral: true
            );
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

    private async Task OnAutocompleteInteraction(SocketAutocompleteInteraction interaction)
    {
        using var scope = logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = Guid.NewGuid(),
                ["InteractionType"] = "Autocomplete",
                ["CommandName"] = interaction.Data.CommandName,
            }
        );

        var context = new SocketInteractionContext(client, interaction);
        await handler.ExecuteCommandAsync(context, provider);
    }

    private async Task OnInteractionExecuted(
        ICommandInfo? commandInfo,
        IInteractionContext context,
        IResult result
    )
    {
        if (result.IsSuccess)
            return;

        if (result is ExecuteResult { Exception: { } ex })
        {
            logger.LogError(
                ex,
                "Unhandled exception occured in interaction {InteractionName}: {Exception}",
                commandInfo?.Name,
                ex.InnerException?.Message
            );
            await context.Interaction.FollowupAsync(
                $"An unhandled exception occured: {ex.InnerException?.Message}"
            );
        }
        else
        {
            logger.LogWarning(
                "Interaction '{InteractionName}' failed: [{Error}] {Reason}",
                commandInfo?.Name,
                result.Error,
                result.ErrorReason
            );
            await context.Interaction.FollowupAsync(
                $"Interaction failed: [{result.Error}] {result.ErrorReason}"
            );
        }
    }
}
