// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Actions.Project.Create;
using Nino.Core.Actions.Project.Delete;
using Nino.Core.Actions.Project.Export;
using Nino.Core.Actions.Project.Resolve;
using Nino.Core.Events;
using Nino.Core.Services;

namespace Nino.Core;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddNinoCore(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<DataContext>(db => db.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        
        // Background Services
        services.AddHostedService<AirNotificationService>();
        
        // Services
        services.AddScoped<IAniListService, AniListService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserVerificationService, UserVerificationService>();
        services.AddScoped<IDataService, DataService>();

        // Action handlers
        services.AddScoped<ProjectResolveHandler>();
        services.AddScoped<ProjectCreateHandler>();
        services.AddScoped<ProjectExportHandler>();
        services.AddScoped<ProjectDeleteHandler>();
        
        return services;
    }
}
