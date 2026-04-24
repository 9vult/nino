// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Projects.GetGenericData.GetGenericProjectDataResponse>;

namespace Nino.Core.Features.Queries.Projects.GetGenericData;

public sealed class GetGenericProjectDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetGenericProjectDataQuery, Result<GetGenericProjectDataResponse>>
{
    public async Task<Result<GetGenericProjectDataResponse>> HandleAsync(
        GetGenericProjectDataQuery query
    )
    {
        var response = await db
            .Projects.Where(p => p.Id == query.ProjectId)
            .Select(p => new GetGenericProjectDataResponse(
                ProjectId: p.Id,
                ProjectTitle: p.Title,
                ProjectType: p.Type,
                AniListId: p.AniListId,
                PosterUrl: p.PosterUrl,
                AniListUrl: p.AniListUrl,
                IsPrivate: p.IsPrivate
            ))
            .FirstOrDefaultAsync();

        return response is not null ? Success(response) : Fail(ResultStatus.NotFound);
    }
}
