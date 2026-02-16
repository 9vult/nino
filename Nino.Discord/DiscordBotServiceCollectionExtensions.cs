// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Events;
using Nino.Discord.Entities;
using Nino.Discord.Handlers;
using Nino.Discord.Services;

namespace Nino.Discord;

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

        // Services
        services.AddScoped<IInteractionIdentityService, InteractionIdentityService>();

        // Handlers!
        services.AddSingleton<InteractionHandler>();
        services.AddScoped<IEventHandler<TaskSkippedEvent>, TaskSkippedEventHandler>();

        services.AddHostedService<DiscordBotHostedService>();
        return services;
    }
}
