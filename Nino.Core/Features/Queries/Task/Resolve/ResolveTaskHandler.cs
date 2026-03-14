// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Task.Resolve;

public sealed class ResolveTaskHandler(NinoDbContext db)
{
    public async Task<Result<TaskId>> ResolveAsync(ResolveTaskQuery query)
    {
        var (episodeId, abbreviation) = query;

        var taskId = await db
            .Tasks.Where(t => t.EpisodeId == episodeId && t.Abbreviation == abbreviation)
            .Select(e => e.Id)
            .SingleOrDefaultAsync();

        return episodeId != default
            ? Result<TaskId>.Success(taskId)
            : Result<TaskId>.Fail(ResultStatus.TaskNotFound);
    }
}
