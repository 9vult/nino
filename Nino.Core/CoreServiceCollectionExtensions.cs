// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Events;
using Nino.Core.Features.Commands;
using Nino.Core.Features.Queries;
using Nino.Core.Services;

namespace Nino.Core;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddCore(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpClient();

        // Database
        services.AddDbContext<NinoDbContext>(ConfigureDb);
        services.AddDbContext<ReadOnlyNinoDbContext>(ConfigureDb);

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

        // Events
        services.AddScoped<IEventBus, InMemoryEventBus>();

        // Services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserVerificationService, UserVerificationService>();
        services.AddScoped<IAniListService, AniListService>();

        return services;

        void ConfigureDb(DbContextOptionsBuilder options) =>
            options.UseSqlite(
                configuration.GetConnectionString("Nino"),
                sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            );
    }
}
