// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Release.Volume;

public sealed class ReleaseVolumeHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    IEventBus eventBus,
    ILogger<ReleaseVolumeHandler> logger
) : ICommandHandler<ReleaseVolumeCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> HandleAsync(ReleaseVolumeCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db
            .Projects.Include(p => p.Observers)
            .Where(p => p.Id == command.ProjectId)
            .Select(p => new
            {
                p.Observers,
                p.DelegateObserverId,
                p.DelegateObserver,
            })
            .FirstOrDefaultAsync();
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        logger.LogInformation(
            "Publishing Release of project {ProjectId} Volume {ReleaseVolume} to local group and {ObserverCount} observers",
            command.ProjectId,
            command.Number,
            project.Observers.Count
        );

        List<Task> tasks =
        [
            eventBus.PublishAsync(
                new VolumeReleasedEvent(
                    ProjectId: command.ProjectId,
                    Number: command.Number,
                    Url: command.Url,
                    Publish: project.DelegateObserver is null,
                    PrimaryRoleId: command.PrimaryRoleId,
                    SecondaryRoleId: command.SecondaryRoleId,
                    TertiaryRoleId: command.TertiaryRoleId
                )
            ),
            .. project.Observers.Select(observer =>
                eventBus.PublishAsync(
                    new VolumeReleasedObserverEvent(
                        ProjectId: command.ProjectId,
                        ObserverId: observer.Id,
                        Number: command.Number,
                        Url: command.Url,
                        Publish: observer.Id == project.DelegateObserverId
                    )
                )
            ),
        ];

        await Task.WhenAll(tasks);
        return Success();
    }
}
