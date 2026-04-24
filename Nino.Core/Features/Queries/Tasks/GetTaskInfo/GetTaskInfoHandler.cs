// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Tasks.GetTaskInfo.GetTaskInfoResponse>;

namespace Nino.Core.Features.Queries.Tasks.GetTaskInfo;

public sealed class GetTaskInfoHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetTaskInfoQuery, Result<GetTaskInfoResponse>>
{
    public async Task<Result<GetTaskInfoResponse>> HandleAsync(GetTaskInfoQuery query)
    {
        var result = await db
            .Tasks.Where(t => t.Id == query.TaskId)
            .Select(t => new GetTaskInfoResponse(
                t.Id,
                t.Abbreviation,
                t.Name,
                t.EpisodeId,
                t.Episode.Number
            ))
            .FirstOrDefaultAsync();

        return result is not null ? Success(result) : Fail(ResultStatus.TaskNotFound);
    }
}
