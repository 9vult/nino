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

        var workingIdx = episodes.FindIndex(e => !e.IsDone);
        if (workingIdx < 0)
            return Fail(ResultStatus.EpisodeNotFound);

        var taskIdx = episodes.FindIndex(e =>
            e.Tasks.Any(t => t.Abbreviation == query.Abbreviation && !t.IsDone)
        );
        if (taskIdx < 0)
            return Fail(ResultStatus.TaskNotFound);

        var task = episodes[taskIdx].Tasks.First(t => t.Abbreviation == query.Abbreviation);

        return Success(
            new GetWorkingTaskEpisodeResponse(
                episodes[workingIdx].Id,
                episodes[taskIdx].Id,
                episodes[workingIdx].Number,
                episodes[taskIdx].Number,
                task.Id,
                task.Name,
                taskIdx - workingIdx
            )
        );
    }
}
