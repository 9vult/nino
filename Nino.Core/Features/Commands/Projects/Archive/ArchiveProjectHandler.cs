// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Archive;

public sealed class ArchiveProjectHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    IEventBus eventBus,
    ILogger<ArchiveProjectHandler> logger
) : ICommandHandler<ArchiveProjectCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(ArchiveProjectCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Owner
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == command.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        project.IsArchived = true;
        await db.SaveChangesAsync();

        var observers = await db
            .Observers.Where(o => o.ProjectId == command.ProjectId)
            .ToListAsync();

        logger.LogInformation(
            "Publishing archival of project {ProjectId} to local group and {ObserverCount} observers",
            command.ProjectId,
            observers.Count
        );

        List<Task> publishTasks =
        [
            eventBus.PublishAsync(new ProjectArchivedEvent(command.ProjectId)),
            .. observers.Select(observer =>
                eventBus.PublishAsync(
                    new ProjectArchivedObserverEvent(observer.Id, command.ProjectId)
                )
            ),
        ];

        await Task.WhenAll(publishTasks);

        return Success();
    }
}
