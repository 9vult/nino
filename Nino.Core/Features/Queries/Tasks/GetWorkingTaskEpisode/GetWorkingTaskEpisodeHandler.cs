// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Tasks.GetWorkingTaskEpisode.GetWorkingTaskEpisodeResponse>;

namespace Nino.Core.Features.Queries.Tasks.GetWorkingTaskEpisode;

public sealed class GetWorkingTaskEpisodeHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetWorkingTaskEpisodeQuery, Result<GetWorkingTaskEpisodeResponse>>
{
    public async Task<Result<GetWorkingTaskEpisodeResponse>> HandleAsync(
        GetWorkingTaskEpisodeQuery query
    )
    {
        var episodes = (
            await db
                .Episodes.Where(e => e.ProjectId == query.ProjectId)
                .Select(e => new
                {
                    e.Id,
                    e.Number,
                    e.Tasks,
                    e.IsDone,
                })
                .ToListAsync()
        ).OrderBy(e => e.Number.Value, StringComparison.OrdinalIgnoreCase.WithNaturalSort()).ToList();

        var taskEpisode = episodes.FirstOrDefault(e =>
            e.Tasks.Any(t => t.Abbreviation == query.Abbreviation && !t.IsDone)
        );

        if (taskEpisode is null)
        {
            var taskExists = episodes.Any(e =>
                e.Tasks.Any(t => t.Abbreviation == query.Abbreviation)
            );
            return taskExists
                ? Fail(ResultStatus.TaskNotFound, message: "all-complete")
                : Fail(ResultStatus.TaskNotFound);
        }

        var workingEpisode = episodes.FirstOrDefault(e => !e.IsDone);
        if (workingEpisode is null)
            return Fail(ResultStatus.EpisodeNotFound);

        var task = taskEpisode.Tasks.First(t => t.Abbreviation == query.Abbreviation);

        return Success(
            new GetWorkingTaskEpisodeResponse(
                workingEpisode.Id,
                taskEpisode.Id,
                workingEpisode.Number,
                taskEpisode.Number,
                task.Id,
                task.Name,
                episodes.IndexOf(taskEpisode) - episodes.IndexOf(workingEpisode)
            )
        );
    }
}
