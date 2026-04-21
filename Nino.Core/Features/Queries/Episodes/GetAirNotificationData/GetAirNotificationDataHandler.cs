// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Episodes.GetAirNotificationData.GetAirNotificationDataResponse>;

namespace Nino.Core.Features.Queries.Episodes.GetAirNotificationData;

public sealed class GetAirNotificationDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetAirNotificationDataQuery, Result<GetAirNotificationDataResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetAirNotificationDataResponse>> HandleAsync(
        GetAirNotificationDataQuery query
    )
    {
        var response = await db
            .Episodes.Where(e => e.Id == query.EpisodeId)
            .Select(e => new GetAirNotificationDataResponse(
                new GetGenericProjectDataResponse(
                    e.ProjectId,
                    e.Project.Title,
                    e.Project.Type,
                    e.Project.PosterUrl,
                    e.Project.AniListUrl,
                    e.Project.IsPrivate
                ),
                e.Number,
                MappedIdDto<ChannelId>.From(e.Project.ProjectChannel),
                MappedIdDto<UserId>.From(e.Project.AirNotificationUser),
                MappedIdDto<RoleId>.From(e.Project.AirNotificationRole),
                e.Group.Configuration.Locale
            ))
            .FirstOrDefaultAsync();

        return response is not null ? Success(response) : Fail(ResultStatus.Error);
    }
}
