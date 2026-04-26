// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Tasks.GetBulkProgressNotificationData.GetBulkProgressNotificationDataResponse>;

namespace Nino.Core.Features.Queries.Tasks.GetBulkProgressNotificationData;

public sealed class GetBulkProgressNotificationDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<
        GetBulkProgressNotificationDataQuery,
        Result<GetBulkProgressNotificationDataResponse>
    >
{
    /// <inheritdoc />
    public async Task<Result<GetBulkProgressNotificationDataResponse>> HandleAsync(
        GetBulkProgressNotificationDataQuery query
    )
    {
        var project = await db
            .Projects.Include(p => p.Episodes)
            .Include(project => project.Group)
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId);

        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        var episodes = project
            .Episodes.OrderBy(
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
            new GetBulkProgressNotificationDataResponse(
                new GetGenericProjectDataResponse(
                    project.Id,
                    project.Title,
                    project.Type,
                    project.AniListId,
                    project.PosterUrl,
                    project.AniListUrl,
                    project.IsPrivate
                ),
                query.Abbreviation,
                taskName,
                firstNumber,
                lastNumber,
                MappedIdDto<ChannelId>.From(project.UpdateChannel),
                project.Group.Configuration.Locale
            )
        );
    }
}
