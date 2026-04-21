// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Tasks.GetCongaNotificationData.GetCongaNotificationDataResponse>;

namespace Nino.Core.Features.Queries.Tasks.GetCongaNotificationData;

public sealed class GetCongaNotificationDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetCongaNotificationDataQuery, Result<GetCongaNotificationDataResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetCongaNotificationDataResponse>> HandleAsync(
        GetCongaNotificationDataQuery query
    )
    {
        var response = await db
            .Episodes.Where(e => e.Id == query.EpisodeId)
            .Select(e => new GetCongaNotificationDataResponse(
                new GetGenericProjectDataResponse(
                    e.ProjectId,
                    e.Project.Title,
                    e.Project.Type,
                    e.Project.PosterUrl,
                    e.Project.AniListUrl,
                    e.Project.IsPrivate
                ),
                e.Project.Nickname,
                e.Number,
                e.Tasks.Where(t => query.TaskIds.Contains(t.Id))
                    .Select(t => new TaskAssigneeDto(t.Name, MappedIdDto<UserId>.From(t.Assignee)))
                    .ToList(),
                MappedIdDto<ChannelId>.From(e.Project.ProjectChannel),
                e.Project.Group.Configuration.CongaPrefixType,
                e.Project.Group.Configuration.Locale
            ))
            .FirstOrDefaultAsync();

        return response is not null ? Success(response) : Fail(ResultStatus.Error);
    }
}
