// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Extensions;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Episodes.List.ListEpisodesResult>>;

namespace Nino.Core.Features.Queries.Episodes.List;

public sealed class ListEpisodesHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListEpisodesQuery, Result<IReadOnlyList<ListEpisodesResult>>>
{
    public async Task<Result<IReadOnlyList<ListEpisodesResult>>> HandleAsync(
        ListEpisodesQuery query
    )
    {
        var episodes = await db.Episodes.Where(e => e.ProjectId == query.ProjectId).ToListAsync();

        episodes = episodes.OrderByNumber().ToList();

        return Success(episodes.Select(e => new ListEpisodesResult(e.Id, e.Number)).ToList());
    }
}
