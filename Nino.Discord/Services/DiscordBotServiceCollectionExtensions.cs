// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Events;
using Nino.Core.Events.Episode;
using Nino.Discord.Handlers;

namespace Nino.Discord.Services;

public static class DiscordBotServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordBotService(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<DiscordSettings>(configuration.GetSection("Discord"));

        services.AddSingleton(
            new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents =
                    GatewayIntents.AllUnprivileged
                    ^ GatewayIntents.GuildScheduledEvents
                    ^ GatewayIntents.GuildInvites,
            }
        );
        services.AddSingleton(p => new InteractionService(
            p.GetRequiredService<DiscordSocketClient>()
        ));
        services.AddSingleton<DiscordSocketClient>();

        // Handlers!
        services.AddSingleton<InteractionHandler>();
        services.AddScoped<IEventHandler<TaskSkippedEvent>, TaskSkippedEventHandler>();

        services.AddHostedService<DiscordBotService>();
        return services;
    }
}
