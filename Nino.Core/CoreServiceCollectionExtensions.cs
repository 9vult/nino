// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nino.Core.Events;
using Nino.Core.Features.AdditionalStaff.Add;
using Nino.Core.Features.AdditionalStaff.Remove;
using Nino.Core.Features.AdditionalStaff.Rename;
using Nino.Core.Features.AdditionalStaff.SetWeight;
using Nino.Core.Features.AdditionalStaff.Swap;
using Nino.Core.Features.Done;
using Nino.Core.Features.Episodes.Add;
using Nino.Core.Features.Episodes.Remove;
using Nino.Core.Features.Episodes.Roster;
using Nino.Core.Features.KeyStaff.Add;
using Nino.Core.Features.KeyStaff.PinchHitter.Remove;
using Nino.Core.Features.KeyStaff.PinchHitter.Set;
using Nino.Core.Features.KeyStaff.Remove;
using Nino.Core.Features.KeyStaff.Rename;
using Nino.Core.Features.KeyStaff.SetWeight;
using Nino.Core.Features.KeyStaff.Swap;
using Nino.Core.Features.Project.Create;
using Nino.Core.Features.Project.Delete;
using Nino.Core.Features.Project.Export;
using Nino.Core.Features.Project.Resolve;
using Nino.Core.Handlers;
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

        // Event handlers
        services.AddScoped<IEventHandler<TaskCompletedEvent>, CongaTaskCompletedEventHandler>();

        // Project handlers
        services.AddScoped<ResolveProjectHandler>();
        services.AddScoped<CreateProjectHandler>();
        services.AddScoped<ExportProjectHandler>();
        services.AddScoped<DeleteProjectHandler>();

        // Key Staff handlers
        services.AddScoped<AddKeyStaffHandler>();
        services.AddScoped<SwapKeyStaffHandler>();
        services.AddScoped<RenameKeyStaffHandler>();
        services.AddScoped<SetKeyStaffWeightHandler>();
        services.AddScoped<RemoveKeyStaffHandler>();
        services.AddScoped<SetPinchHitterHandler>();
        services.AddScoped<RemovePinchHitterHandler>();

        // Additional Staff handlers
        services.AddScoped<AddAdditionalStaffHandler>();
        services.AddScoped<SwapAdditionalStaffHandler>();
        services.AddScoped<RenameAdditionalStaffHandler>();
        services.AddScoped<SetAdditionalStaffWeightHandler>();
        services.AddScoped<RemoveAdditionalStaffHandler>();

        // Episode handlers
        services.AddScoped<AddEpisodeHandler>();
        services.AddScoped<RemoveEpisodeHandler>();
        services.AddScoped<EpisodeRosterHandler>();

        // Task handlers
        services.AddScoped<DoneHandler>();

        return services;
    }
}
