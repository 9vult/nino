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
        task.UpdatedAt = DateTimeOffset.UtcNow;
        task.Episode.UpdatedAt = DateTimeOffset.UtcNow;
        task.Episode.IsDone = task.Episode.Tasks.All(t => t.IsDone);
        await db.SaveChangesAsync();

        await eventBus.PublishAsync(
            new TaskProgressCongaEvent(
                ProjectId: command.ProjectId,
                EpisodeId: command.EpisodeId,
                TaskId: command.TaskId,
                ProgressType.Done
            )
        );

        var shouldPublish =
            !task.IsPseudo
            && await db
                .Projects.Where(p => p.Id == command.ProjectId)
                .Select(p => !p.IsPrivate || p.Group.Configuration.PublishPrivateProgress)
                .FirstOrDefaultAsync();

        if (!shouldPublish)
        {
            logger.LogInformation(
                "Skipping publish of skip of task {TaskId} due to pseudo state or group config",
                command.TaskId
            );
            return Success(task.Episode.Tasks.All(t => t.IsDone));
        }

        var observers = await db
            .Observers.Where(o => o.ProjectId == command.ProjectId)
            .ToListAsync();

        logger.LogInformation(
            "Publishing skip of task {TaskId} to local group and {ObserverCount} observers",
            task.Id,
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
