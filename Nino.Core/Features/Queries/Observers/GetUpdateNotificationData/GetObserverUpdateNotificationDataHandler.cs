// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Observers.GetUpdateNotificationData.GetObserverUpdateNotificationDataResponse>;

namespace Nino.Core.Features.Queries.Observers.GetUpdateNotificationData;

public sealed class GetObserverUpdateNotificationDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<
        GetObserverUpdateNotificationDataQuery,
        Result<GetObserverUpdateNotificationDataResponse>
    >
{
    /// <inheritdoc />
    public async Task<Result<GetObserverUpdateNotificationDataResponse>> HandleAsync(
        GetObserverUpdateNotificationDataQuery query
    )
    {
        var taskInfo = await db
            .Tasks.Where(t => t.Id == query.TaskId)
            .Select(t => new
            {
                t.Abbreviation,
                t.Name,
                t.Episode.Number,
            })
            .FirstOrDefaultAsync();
        if (taskInfo is null)
            return Fail(ResultStatus.TaskNotFound);

        var response = await db
            .Observers.Where(p => p.Id == query.ObserverId)
            .Select(o => new GetObserverUpdateNotificationDataResponse(
                new GetGenericProjectDataResponse(
                    o.ProjectId,
                    o.Project.Title,
                    o.Project.Type,
                    o.Project.AniListId,
                    o.Project.PosterUrl,
                    o.Project.AniListUrl,
                    o.Project.IsPrivate
                ),
                taskInfo.Abbreviation,
                taskInfo.Name,
                taskInfo.Number,
                o.Group.Configuration.ProgressPublishType,
                MappedIdDto<ChannelId>.From(o.UpdateChannel),
                o.Group.Configuration.Locale
            ))
            .FirstOrDefaultAsync();

        return response is not null ? Success(response) : Fail(ResultStatus.ObserverNotFound);
    }
}
