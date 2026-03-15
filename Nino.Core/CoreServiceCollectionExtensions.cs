// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Features.Commands.AdditionalStaff.Add;
using Nino.Core.Features.Commands.AdditionalStaff.Rename;
using Nino.Core.Features.Commands.AdditionalStaff.SetWeight;
using Nino.Core.Features.Commands.AdditionalStaff.Swap;
using Nino.Core.Features.Commands.Episodes.Add;
using Nino.Core.Features.Commands.Episodes.Remove;
using Nino.Core.Features.Commands.KeyStaff.Add;
using Nino.Core.Features.Commands.KeyStaff.PinchHitter.Remove;
using Nino.Core.Features.Commands.KeyStaff.PinchHitter.Set;
using Nino.Core.Features.Commands.KeyStaff.Remove;
using Nino.Core.Features.Commands.KeyStaff.Rename;
using Nino.Core.Features.Commands.KeyStaff.SetWeight;
using Nino.Core.Features.Commands.KeyStaff.Swap;
using Nino.Core.Features.Commands.Project.Create;
using Nino.Core.Features.Commands.Project.Edit;
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
        // HttpClient
        services.AddHttpClient();

        // Database
        services.AddDbContext<NinoDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("Nino"),
                sqliteOptions =>
                    sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            )
        );

        // Services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserVerificationService, UserVerificationService>();
        services.AddScoped<IAniListService, AniListService>();
        services.AddScoped<IStateService, StateService>();

        // Command handlers
        // Project
        services.AddScoped<CreateProjectHandler>();
        services.AddScoped<EditProjectHandler>();

        // Key Staff
        services.AddScoped<AddKeyStaffHandler>();
        services.AddScoped<RenameKeyStaffHandler>();
        services.AddScoped<SwapKeyStaffHandler>();
        services.AddScoped<SetKeyStaffWeightHandler>();
        services.AddScoped<RemoveKeyStaffHandler>();
        services.AddScoped<SetPinchHitterHandler>();
        services.AddScoped<RemovePinchHitterHandler>();

        // Additional Staff
        services.AddScoped<AddAdditionalStaffHandler>();
        services.AddScoped<RenameAdditionalStaffHandler>();
        services.AddScoped<SwapAdditionalStaffHandler>();
        services.AddScoped<SetAdditionalStaffWeightHandler>();
        services.AddScoped<RemoveKeyStaffHandler>();

        // Episodes
        services.AddScoped<AddEpisodeHandler>();
        services.AddScoped<RemoveEpisodeHandler>();

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
