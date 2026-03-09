// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddHostedService<DiscordBotHostedService>();
        return services;
    }
}
