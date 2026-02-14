// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Nino.Discord.Services;

public class DiscordBotService(ILogger<DiscordBotService> logger, IOptions<DiscordSettings> options)
    : IHostedService
{
    private static readonly DiscordSocketConfig SocketConfig = new()
    {
        LogLevel = LogSeverity.Info,
        GatewayIntents =
            GatewayIntents.AllUnprivileged
            ^ GatewayIntents.GuildScheduledEvents
            ^ GatewayIntents.GuildInvites,
    };
    private readonly DiscordSocketClient _client = new(SocketConfig);
    private readonly DiscordSettings _settings = options.Value;

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Nino Discord Bot...");
        _client.Ready += OnReady;
        _client.Log += OnLog;

        await _client.LoginAsync(TokenType.Bot, _settings.Token);
        await _client.StartAsync();
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Nino Discord Bot...");

        await _client.LogoutAsync();
        await _client.StopAsync();
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
            _client.CurrentUser.Username
        );
        return Task.CompletedTask;
    }
}
