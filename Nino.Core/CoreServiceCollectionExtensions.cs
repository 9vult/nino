// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Events;
using Nino.Core.Features.KeyStaff.Add;
using Nino.Core.Features.KeyStaff.Remove;
using Nino.Core.Features.Project.Create;
using Nino.Core.Features.Project.Delete;
using Nino.Core.Features.Project.Export;
using Nino.Core.Features.Project.Resolve;
using Nino.Core.Services;

namespace Nino.Core;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddNinoCore(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<DataContext>(db =>
            db.UseSqlite(configuration.GetConnectionString("DefaultConnection"))
        );
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        // Background Services
        services.AddHostedService<AirNotificationService>();

        // Services
        services.AddScoped<IAniListService, AniListService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserVerificationService, UserVerificationService>();
        services.AddScoped<IDataService, DataService>();
        services.AddScoped<IStateService, StateService>();

        // Project handlers
        services.AddScoped<ResolveProjectHandler>();
        services.AddScoped<CreateProjectHandler>();
        services.AddScoped<ExportProjectHandler>();
        services.AddScoped<DeleteProjectHandler>();

        // Key Staff handlers
        services.AddScoped<AddKeyStaffHandler>();
        services.AddScoped<RemoveKeyStaffHandler>();

        return services;
    }
}
