// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Tasks.ListForEpisode.ListTasksForEpisodeResult>>;

namespace Nino.Core.Features.Queries.Tasks.ListForEpisode;

public sealed class ListTasksForEpisodeHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListTasksForEpisodeQuery, Result<IReadOnlyList<ListTasksForEpisodeResult>>>
{
    public async Task<Result<IReadOnlyList<ListTasksForEpisodeResult>>> HandleAsync(
        ListTasksForEpisodeQuery query
    )
    {
        var staff = await db
            .Tasks.Where(s => s.EpisodeId == query.EpisodeId)
            .OrderBy(s => s.Abbreviation)
            .Select(s => new ListTasksForEpisodeResult(s.Id, s.Abbreviation))
            .ToListAsync();

        return Success(staff);
    }
}
