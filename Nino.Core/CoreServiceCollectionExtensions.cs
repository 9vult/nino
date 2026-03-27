// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features.Commands;
using Nino.Core.Features.Queries;

namespace Nino.Core;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddCore(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpClient();

        // Add command, query handlers
        var handlers = typeof(CoreServiceCollectionExtensions)
            .Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t =>
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType
                        && (
                            i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
                            || i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                        )
                    )
            )
            .ToList();
        foreach (var handler in handlers)
            services.AddScoped(handler);

        return services;
    }
}
