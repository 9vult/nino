// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Episodes.GetWorkingEpisode.GetWorkingEpisodeResponse>;

namespace Nino.Core.Features.Queries.Episodes.GetWorkingEpisode;

public sealed class GetWorkingEpisodeHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetWorkingEpisodeQuery, Result<GetWorkingEpisodeResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetWorkingEpisodeResponse>> HandleAsync(GetWorkingEpisodeQuery query)
    {
        var episodes = await db
            .Episodes.Where(e => e.ProjectId == query.ProjectId)
            .Select(e => new
            {
                e.Id,
                e.Number,
                e.IsDone,
            })
            .ToListAsync();

        if (episodes.Count == 0)
            return Fail(ResultStatus.ProjectNotFound);

        var ordered = episodes.OrderBy(
            e => e.Number.Value,
            StringComparer.OrdinalIgnoreCase.WithNaturalSort()
        );

        foreach (var episode in ordered)
        {
            if (!episode.IsDone)
            {
                return Success(new GetWorkingEpisodeResponse(episode.Id, episode.Number));
            }
        }
        return Fail(ResultStatus.EpisodeNotFound, message: "all-complete");
    }
}
