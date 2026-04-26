// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Observers.GetBulkUpdateNotificationData.GetBulkObserverUpdateNotificationDataResponse>;

namespace Nino.Core.Features.Queries.Observers.GetBulkUpdateNotificationData;

public sealed class GetBulkObserverUpdateNotificationDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<
        GetBulkObserverUpdateNotificationDataQuery,
        Result<GetBulkObserverUpdateNotificationDataResponse>
    >
{
    /// <inheritdoc />
    public async Task<Result<GetBulkObserverUpdateNotificationDataResponse>> HandleAsync(
        GetBulkObserverUpdateNotificationDataQuery query
    )
    {
        var observer = await db
            .Observers.Include(o => o.Project)
            .ThenInclude(p => p.Episodes)
            .Include(o => o.Group)
            .Include(observer => observer.UpdateChannel)
            .FirstOrDefaultAsync(p => p.Id == query.ObserverId);

        if (observer is null)
            return Fail(ResultStatus.ObserverNotFound);

        var episodes = observer
            .Project.Episodes.OrderBy(
                e => e.Number.Value,
                StringComparison.OrdinalIgnoreCase.WithNaturalSort()
            )
            .ToList();

        var firstIdx = episodes.FindIndex(e => e.Id == query.FirstEpisodeId);
        var lastIdx = episodes.FindIndex(e => e.Id == query.LastEpisodeId) + 1;

        if (firstIdx < 0)
            return Fail(ResultStatus.EpisodeNotFound, "first");
        if (lastIdx < 1)
            return Fail(ResultStatus.EpisodeNotFound, "last");

        episodes = episodes[firstIdx..lastIdx];
        var firstNumber = episodes.First().Number;
        var lastNumber = episodes.Last().Number;
        var taskName =
            episodes
                .SelectMany(e => e.Tasks)
                .FirstOrDefault(t => t.Abbreviation == query.Abbreviation)
                ?.Name
            ?? "Unknown";

        return Success(
            new GetBulkObserverUpdateNotificationDataResponse(
                new GetGenericProjectDataResponse(
                    observer.Project.Id,
                    observer.Project.Title,
                    observer.Project.Type,
                    observer.Project.AniListId,
                    observer.Project.PosterUrl,
                    observer.Project.AniListUrl,
                    observer.Project.IsPrivate
                ),
                query.Abbreviation,
                taskName,
                firstNumber,
                lastNumber,
                MappedIdDto<ChannelId>.From(observer.UpdateChannel),
                observer.Group.Configuration.Locale
            )
        );
    }
}
