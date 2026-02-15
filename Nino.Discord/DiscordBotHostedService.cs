// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nino.Discord.Entities;
using Nino.Discord.Handlers;

namespace Nino.Discord;

public class DiscordBotHostedService(
    DiscordSocketClient client,
    InteractionHandler interactionHandler,
    ILogger<DiscordBotHostedService> logger,
    IOptions<DiscordSettings> options
) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Nino Discord Bot...");
        client.Ready += OnReady;
        client.Log += OnLog;

        await interactionHandler.InitializeAsync();

        await client.LoginAsync(TokenType.Bot, options.Value.Token);
        await client.StartAsync();
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Nino Discord Bot...");

        await client.LogoutAsync();
        await client.StopAsync();
    }

    private Task OnLog(LogMessage msg)
    {
        logger.Log(
            logLevel: msg.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Trace,
                _ => LogLevel.Information,
            },
            exception: msg.Exception,
            message: "{DiscordMessage}",
            args: msg.Message
        );
        return Task.CompletedTask;
    }

    private Task OnReady()
    {
        logger.LogInformation(
            "Successfully logged in to Discord as {Username}",
            client.CurrentUser.Username
        );
        return Task.CompletedTask;
    }
}
