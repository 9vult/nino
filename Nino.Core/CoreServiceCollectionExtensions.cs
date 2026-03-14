// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features.Commands.AdditionalStaff.Add;
using Nino.Core.Features.Commands.AdditionalStaff.Rename;
using Nino.Core.Features.Commands.AdditionalStaff.Swap;
using Nino.Core.Features.Commands.KeyStaff.Add;
using Nino.Core.Features.Commands.KeyStaff.Rename;
using Nino.Core.Features.Commands.KeyStaff.Swap;
using Nino.Core.Features.Commands.Project.Create;
using Nino.Core.Features.Queries.Episode.Resolve;
using Nino.Core.Features.Queries.Project.Resolve;
using Nino.Core.Features.Queries.Project.Status;
using Nino.Core.Features.Queries.Staff.Resolve;
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
            options.UseSqlite(
                configuration.GetConnectionString("Nino"),
                sqliteOptions =>
                    sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            )
        );
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserVerificationService, UserVerificationService>();
        services.AddScoped<IAniListService, AniListService>();
        services.AddScoped<IStateService, StateService>();

        // Command handlers
        // Project
        services.AddScoped<CreateProjectHandler>();

        // Key Staff
        services.AddScoped<AddKeyStaffHandler>();
        services.AddScoped<RenameKeyStaffHandler>();
        services.AddScoped<SwapKeyStaffHandler>();

        // Additional Staff
        services.AddScoped<AddAdditionalStaffHandler>();
        services.AddScoped<RenameAdditionalStaffHandler>();
        services.AddScoped<SwapAdditionalStaffHandler>();

        // Query handlers
        // Resolvers
        services.AddScoped<ResolveProjectHandler>();
        services.AddScoped<ResolveEpisodeHandler>();
        services.AddScoped<ResolveTaskHandler>();
        services.AddScoped<ResolveStaffHandler>();

        services.AddScoped<ProjectStatusHandler>();

        return services;
    }
}
