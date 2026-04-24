// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result<bool>;

namespace Nino.Core.Features.Commands.Tasks.MarkDone;

public class MarkTaskDoneHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    IEventBus eventBus,
    ILogger<MarkTaskDoneHandler> logger
) : ICommandHandler<MarkTaskDoneCommand, Result<bool>>
{
    /// <inheritdoc />
    public async Task<Result<bool>> HandleAsync(MarkTaskDoneCommand command)
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

        if (
            !await db
                .Projects.Where(p => p.Id == command.ProjectId)
                .Select(p => p.Group.Configuration.PublishPrivateProgress)
                .FirstOrDefaultAsync()
        )
        {
            return Success(task.Episode.Tasks.All(t => t.IsDone));
        }

        var observers = await db
            .Observers.Where(o => o.ProjectId == command.ProjectId)
            .ToListAsync();

        logger.LogInformation(
            "Publishing completion of project {ProjectId} Episode {EpisodeId}'s {Abbreviation} to local group and {ObserverCount} observers",
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
                    ProgressType.Done
                )
            ),
            .. observers.Select(observer =>
                eventBus.PublishAsync(
                    new TaskProgressObserverEvent(
                        ObserverId: observer.Id,
                        ProjectId: command.ProjectId,
                        EpisodeId: command.EpisodeId,
                        TaskId: command.TaskId,
                        ProgressType.Done
                    )
                )
            ),
        ];

        await Task.WhenAll(publishTasks);
        return Success(task.Episode.Tasks.All(t => t.IsDone));
    }
}
