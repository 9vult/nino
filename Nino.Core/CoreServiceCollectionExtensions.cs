// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features.Queries.Project.Resolve;
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

        // Services
        services.AddDbContext<NinoDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Nino"))
        );
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserVerificationService, UserVerificationService>();

        // Command handlers

        // Query handlers
        services.AddScoped<ResolveProjectHandler>();

        return services;
    }
}
