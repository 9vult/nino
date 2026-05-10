// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Observers.GetGenericObserverData.GetGenericObserverDataResponse>;

namespace Nino.Core.Features.Queries.Observers.GetGenericObserverData;

public class GetGenericObserverDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetGenericObserverDataQuery, Result<GetGenericObserverDataResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetGenericObserverDataResponse>> HandleAsync(
        GetGenericObserverDataQuery query
    )
    {
        var result = await db
            .Observers.Where(o => o.Id == query.ObserverId)
            .Select(o => new GetGenericObserverDataResponse(
                ProjectData: new GetGenericProjectDataResponse(
                    o.Project.Id,
                    MappedIdDto<UserId>.From(o.Project.Owner),
                    o.Project.Title,
                    o.Project.Type,
                    o.Project.AniListId,
                    o.Project.PosterUrl,
                    o.Project.AniListUrl,
                    o.Project.IsPrivate
                ),
                Owner: MappedIdDto<UserId>.From(o.Owner)
            ))
            .FirstOrDefaultAsync();

        return result is not null ? Success(result) : Fail(ResultStatus.ObserverNotFound);
    }
}
