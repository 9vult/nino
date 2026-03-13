// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features.Commands.KeyStaff.Add;
using Nino.Core.Features.Commands.Project.Create;
using Nino.Core.Features.Queries.Episode.Resolve;
using Nino.Core.Features.Queries.Project.Resolve;
using Nino.Core.Features.Queries.Task.Resolve;
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
        services.AddScoped<IAniListService, AniListService>();

        // Command handlers
        // Project
        services.AddScoped<CreateProjectHandler>();

        // Key Staff
        services.AddScoped<AddKeyStaffHandler>();

        // Query handlers
        services.AddScoped<ResolveProjectHandler>();
        services.AddScoped<ResolveEpisodeHandler>();
        services.AddScoped<ResolveTaskHandler>();

        return services;
    }
}
