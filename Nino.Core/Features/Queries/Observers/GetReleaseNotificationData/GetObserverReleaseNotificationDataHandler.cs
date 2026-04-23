// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Observers.GetReleaseNotificationData.GetObserverReleaseNotificationDataResponse>;

namespace Nino.Core.Features.Queries.Observers.GetReleaseNotificationData;

public sealed class GetObserverReleaseNotificationDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<
        GetObserverReleaseNotificationDataQuery,
        Result<GetObserverReleaseNotificationDataResponse>
    >
{
    /// <inheritdoc />
    public async Task<Result<GetObserverReleaseNotificationDataResponse>> HandleAsync(
        GetObserverReleaseNotificationDataQuery query
    )
    {
        var response = await db
            .Observers.Where(p => p.Id == query.ObserverId)
            .Select(o => new GetObserverReleaseNotificationDataResponse(
                o.Project.Title,
                MappedIdDto<ChannelId>.From(o.ReleaseChannel),
                MappedIdDto<RoleId>.From(o.PrimaryRole),
                MappedIdDto<RoleId>.From(o.SecondaryRole),
                MappedIdDto<RoleId>.From(o.TertiaryRole),
                o.Group.Configuration.Locale
            ))
            .FirstOrDefaultAsync();

        return response is not null ? Success(response) : Fail(ResultStatus.ObserverNotFound);
    }
}
