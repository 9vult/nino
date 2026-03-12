// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Discord.Handlers;
using Nino.Discord.Services;

namespace Nino.Discord;

public static class DiscordBotServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordBot(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptionsWithValidateOnStart<DiscordOptions>()
            .BindConfiguration(DiscordOptions.Section)
            .ValidateDataAnnotations();

        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(p => new InteractionService(
            p.GetRequiredService<DiscordSocketClient>()
        ));

        // Services
        services.AddScoped<IInteractionIdentityService, InteractionIdentityService>();
        services.AddScoped<IBotPermissionsService, BotPermissionsService>();

        // Handlers
        services.AddSingleton<InteractionHandler>();

        services.AddHostedService<DiscordBotHostedService>();
        return services;
    }
}
