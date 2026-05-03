// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Domain.ValueObjects.TaskId>;

namespace Nino.Core.Features.Queries.Tasks.Resolve;

public sealed class ResolveTaskHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ResolveTaskQuery, Result<TaskId>>
{
    public async Task<Result<TaskId>> HandleAsync(ResolveTaskQuery query)
    {
        var (episodeId, abbreviation) = query;

        var taskId = await db
            .Tasks.Where(t => t.EpisodeId == episodeId && t.Abbreviation == abbreviation)
            .Select(t => (TaskId?)t.Id)
            .FirstOrDefaultAsync();

        return taskId is not null ? Success(taskId.Value) : Fail(ResultStatus.TaskResolutionFailed);
    }
}
