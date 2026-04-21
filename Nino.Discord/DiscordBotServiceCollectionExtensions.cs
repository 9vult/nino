// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Events;
using Nino.Discord.Handlers;
using Nino.Discord.Interactions;
using Nino.Discord.Services;
using Nino.Domain.ValueObjects;

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
        services.AddSingleton(p =>
        {
            var interactionService = new InteractionService(
                p.GetRequiredService<DiscordSocketClient>()
            );

            interactionService.AddTypeConverter<Alias>(new VogenTypeConverter<Alias>());
            interactionService.AddTypeConverter<Abbreviation>(
                new VogenTypeConverter<Abbreviation>()
            );
            interactionService.AddTypeConverter<Number>(new VogenTypeConverter<Number>());

            return interactionService;
        });

        // Services
        services.AddScoped<IInteractionIdentityService, InteractionIdentityService>();
        services.AddScoped<IBotPermissionsService, BotPermissionsService>();

        // Handlers
        services.AddSingleton<InteractionHandler>();
        services.AddScoped<IEventHandler<EpisodeAiredEvent>, EpisodeAiredEventHandler>();
        services.AddScoped<IEventHandler<CongaNotificationEvent>, CongaNotificationEventHandler>();
        services.AddScoped<
            IEventHandler<PartialGroupCreatedFromDiscordEvent>,
            PartialGroupCreatedEventHandler
        >();

        services.AddHostedService<DiscordBotHostedService>();
        return services;
    }
}
