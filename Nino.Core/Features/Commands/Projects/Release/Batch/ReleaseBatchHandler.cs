// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Release.Batch;

public sealed class ReleaseBatchHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    IEventBus eventBus,
    ILogger<ReleaseBatchHandler> logger
) : ICommandHandler<ReleaseBatchCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> HandleAsync(ReleaseBatchCommand command)
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
            "Publishing Release of project {ProjectId} Batch ({ReleaseEpisodeFirst}-{ReleaseEpisodeLast}) to local group and {ObserverCount} observers",
            command.ProjectId,
            command.FirstNumber,
            command.LastNumber,
            project.Observers.Count
        );

        List<Task> tasks =
        [
            eventBus.PublishAsync(
                new BatchReleasedEvent(
                    ProjectId: command.ProjectId,
                    FirstNumber: command.FirstNumber,
                    LastNumber: command.LastNumber,
                    Url: command.Url,
                    Publish: project.DelegateObserver is null,
                    PrimaryRoleId: command.PrimaryRoleId,
                    SecondaryRoleId: command.SecondaryRoleId,
                    TertiaryRoleId: command.TertiaryRoleId
                )
            ),
            .. project.Observers.Select(observer =>
                eventBus.PublishAsync(
                    new BatchReleasedObserverEvent(
                        ProjectId: command.ProjectId,
                        ObserverId: observer.Id,
                        FirstNumber: command.FirstNumber,
                        LastNumber: command.LastNumber,
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
