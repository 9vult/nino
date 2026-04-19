// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Tasks.List.ListTasksResult>>;

namespace Nino.Core.Features.Queries.Tasks.List;

public sealed class ListTasksHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListTasksQuery, Result<IReadOnlyList<ListTasksResult>>>
{
    public async Task<Result<IReadOnlyList<ListTasksResult>>> HandleAsync(ListTasksQuery query)
    {
        var staff = await db
            .Tasks.Where(s => s.EpisodeId == query.EpisodeId)
            .OrderBy(s => s.Abbreviation)
            .Select(s => new ListTasksResult(s.Id, s.Abbreviation))
            .ToListAsync();

        return Success(staff);
    }
}
