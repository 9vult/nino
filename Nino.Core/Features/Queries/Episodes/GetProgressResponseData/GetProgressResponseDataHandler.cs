// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Episodes.GetProgressResponseData.GetProgressResponseDataResponse>;

namespace Nino.Core.Features.Queries.Episodes.GetProgressResponseData;

public sealed class GetProgressResponseDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetProgressResponseDataQuery, Result<GetProgressResponseDataResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetProgressResponseDataResponse>> HandleAsync(
        GetProgressResponseDataQuery query
    )
    {
        var episodeId = query.EpisodeId;

        var result = await db
            .Episodes.Where(e => e.Id == episodeId)
            .Select(e => new GetProgressResponseDataResponse(
                e.Group.Configuration.ProgressResponseType,
                e.Tasks.Select(t => new GetProgressResponseDataStatus(
                        t.Abbreviation,
                        t.Name,
                        t.IsDone,
                        t.Weight,
                        t.IsPseudo
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync();

        return result is not null ? Success(result) : Fail(ResultStatus.EpisodeNotFound);
    }
}
