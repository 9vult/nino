// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Projects.GetReleaseNotificationData.GetReleaseNotificationDataResponse>;

namespace Nino.Core.Features.Queries.Projects.GetReleaseNotificationData;

public sealed class GetReleaseNotificationDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetReleaseNotificationDataQuery, Result<GetReleaseNotificationDataResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetReleaseNotificationDataResponse>> HandleAsync(
        GetReleaseNotificationDataQuery query
    )
    {
        var response = await db
            .Projects.Where(p => p.Id == query.ProjectId)
            .Select(p => new
            {
                p.Title,
                p.Group.Configuration.ReleasePrefix,
                p.ReleaseChannel,
                p.Group.Configuration.Locale,
            })
            .FirstOrDefaultAsync();
        if (response is null)
            return Fail(ResultStatus.ProjectNotFound);

        var primaryRole = query.PrimaryRoleId is not null
            ? await db.Roles.FirstOrDefaultAsync(r => r.Id == query.PrimaryRoleId)
            : null;
        var secondaryRole = query.SecondaryRoleId is not null
            ? await db.Roles.FirstOrDefaultAsync(r => r.Id == query.SecondaryRoleId)
            : null;
        var tertiaryRole = query.TertiaryRoleId is not null
            ? await db.Roles.FirstOrDefaultAsync(r => r.Id == query.TertiaryRoleId)
            : null;

        return Success(
            new GetReleaseNotificationDataResponse(
                response.Title,
                response.ReleasePrefix,
                MappedIdDto<ChannelId>.From(response.ReleaseChannel),
                MappedIdDto<RoleId>.From(primaryRole),
                MappedIdDto<RoleId>.From(secondaryRole),
                MappedIdDto<RoleId>.From(tertiaryRole),
                response.Locale
            )
        );
    }
}
