// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result<bool>;

namespace Nino.Core.Features.Commands.Tasks.MarkSkipped;

public class MarkTaskSkippedHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    IEventBus eventBus,
    ILogger<MarkTaskSkippedHandler> logger
) : ICommandHandler<MarkTaskSkippedCommand, Result<bool>>
{
    /// <inheritdoc />
    public async Task<Result<bool>> HandleAsync(MarkTaskSkippedCommand command)
    {
        var verification = await verificationService.VerifyTaskPermissionsAsync(
            command.ProjectId,
            command.EpisodeId,
            command.TaskId,
            command.RequestedBy
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var task = await db
            .Tasks.Include(t => t.Episode)
            .FirstOrDefaultAsync(t => t.Id == command.TaskId);

        if (task is null)
            return Fail(ResultStatus.TaskNotFound);

        if (task.IsDone)
            return Fail(ResultStatus.BadRequest);

        task.IsDone = true;
        await db.SaveChangesAsync();

        var observers = await db
            .Observers.Where(o => o.ProjectId == command.ProjectId)
            .ToListAsync();

        logger.LogInformation(
            "Publishing skip of project {ProjectId} Episode {EpisodeId}'s {Abbreviation} to local group and {ObserverCount} observers",
            command.ProjectId,
            command.EpisodeId,
            task.Abbreviation,
            observers.Count
        );

        List<Task> publishTasks =
        [
            eventBus.PublishAsync(
                new TaskProgressEvent(
                    ProjectId: command.ProjectId,
                    EpisodeId: command.EpisodeId,
                    TaskId: command.TaskId,
                    ProgressType.Skipped
                )
            ),
            .. observers.Select(observer =>
                eventBus.PublishAsync(
                    new TaskProgressObserverEvent(
                        ObserverId: observer.Id,
                        ProjectId: command.ProjectId,
                        EpisodeId: command.EpisodeId,
                        TaskId: command.TaskId,
                        ProgressType.Skipped
                    )
                )
            ),
        ];

        await Task.WhenAll(publishTasks);
        return Success(task.Episode.Tasks.All(t => t.IsDone));
    }
}
