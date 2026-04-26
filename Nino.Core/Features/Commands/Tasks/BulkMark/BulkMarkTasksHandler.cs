// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;
using Nino.Core.Events;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Commands.Tasks.BulkMark.BulkMarkTasksResponse>;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Features.Commands.Tasks.BulkMark;

public sealed class BulkMarkTasksHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    IEventBus eventBus,
    ILogger<BulkMarkTasksHandler> logger
) : ICommandHandler<BulkMarkTasksCommand, Result<BulkMarkTasksResponse>>
{
    /// <inheritdoc />
    public async Task<Result<BulkMarkTasksResponse>> HandleAsync(BulkMarkTasksCommand command)
    {
        var verificationTask = await db.Tasks.FirstOrDefaultAsync(t =>
            t.EpisodeId == command.FirstEpisodeId && t.Abbreviation == command.Abbreviation
        );

        if (verificationTask is null)
            return Fail(ResultStatus.TaskNotFound);

        var verification = await verificationService.VerifyTaskPermissionsAsync(
            command.ProjectId,
            command.FirstEpisodeId,
            verificationTask.Id,
            command.RequestedBy
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var episodes = (
            await db.Episodes.Where(e => e.ProjectId == command.ProjectId).ToListAsync()
        )
            .OrderBy(e => e.Number.Value, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        var firstIdx = episodes.FindIndex(e => e.Id == command.FirstEpisodeId);
        var lastIdx = episodes.FindIndex(e => e.Id == command.LastEpisodeId) + 1;

        if (firstIdx < 0)
            return Fail(ResultStatus.EpisodeNotFound, "first");
        if (lastIdx < 1)
            return Fail(ResultStatus.EpisodeNotFound, "last");

        episodes = episodes[firstIdx..lastIdx].ToList();

        List<(EpisodeId, Number)> completedEpisodes = [];

        foreach (var episode in episodes)
        {
            var task = episode.Tasks.FirstOrDefault(t => t.Abbreviation == command.Abbreviation);
            if (task is null)
                continue;

            task.IsDone = command.ProgressType is ProgressType.Done or ProgressType.Skipped;
            task.UpdatedAt = DateTimeOffset.UtcNow;
            task.Episode.UpdatedAt = DateTimeOffset.UtcNow;

            episode.IsDone = episode.Tasks.All(t => t.IsDone);

            if (episode.IsDone)
                completedEpisodes.Add((episode.Id, episode.Number));
        }
        await db.SaveChangesAsync();

        var observers = await db
            .Observers.Where(o => o.ProjectId == command.ProjectId)
            .ToListAsync();

        logger.LogInformation(
            "Publishing bulk {ProgressType} of project {ProjectId} Episode {FirstEpisodeId} thru {LastEpisodeId} {Abbreviation} to local group and {ObserverCount} observers",
            command.ProgressType,
            command.ProjectId,
            command.FirstEpisodeId,
            command.LastEpisodeId,
            command.Abbreviation,
            observers.Count
        );

        List<Task> publishTasks =
        [
            eventBus.PublishAsync(
                new BulkTaskProgressEvent(
                    ProjectId: command.ProjectId,
                    FirstEpisodeId: command.FirstEpisodeId,
                    LastEpisodeId: command.LastEpisodeId,
                    Abbreviation: command.Abbreviation,
                    ProgressType.Done
                )
            ),
            .. observers.Select(observer =>
                eventBus.PublishAsync(
                    new BulkTaskProgressObserverEvent(
                        ObserverId: observer.Id,
                        ProjectId: command.ProjectId,
                        FirstEpisodeId: command.FirstEpisodeId,
                        LastEpisodeId: command.LastEpisodeId,
                        Abbreviation: command.Abbreviation,
                        ProgressType.Done
                    )
                )
            ),
        ];

        await Task.WhenAll(publishTasks);
        return Success(new BulkMarkTasksResponse(completedEpisodes));
    }
}
