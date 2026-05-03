// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Tasks.GetProgressNotificationData.GetProgressNotificationDataResponse>;

namespace Nino.Core.Features.Queries.Tasks.GetProgressNotificationData;

public sealed class GetProgressNotificationDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetProgressNotificationDataQuery, Result<GetProgressNotificationDataResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetProgressNotificationDataResponse>> HandleAsync(
        GetProgressNotificationDataQuery query
    )
    {
        var result = await db
            .Tasks.Where(t => t.Id == query.TaskId)
            .Select(t => new GetProgressNotificationDataResponse(
                new GetGenericProjectDataResponse(
                    t.ProjectId,
                    t.Project.Title,
                    t.Project.Type,
                    t.Project.AniListId,
                    t.Project.PosterUrl,
                    t.Project.AniListUrl,
                    t.Project.IsPrivate
                ),
                t.Abbreviation,
                t.Name,
                t.Episode.Number,
                t.Project.Group.Configuration.ProgressPublishType,
                MappedIdDto<ChannelId>.From(t.Project.UpdateChannel),
                t.Project.Group.Configuration.Locale
            ))
            .FirstOrDefaultAsync();

        return result is not null ? Success(result) : Fail(ResultStatus.TaskNotFound);
    }
}
