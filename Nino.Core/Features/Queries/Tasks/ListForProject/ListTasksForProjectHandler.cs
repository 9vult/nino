// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Tasks.ListForProject.ListTasksForProjectResult>>;

namespace Nino.Core.Features.Queries.Tasks.ListForProject;

public sealed class ListTasksForProjectHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListTasksForProjectQuery, Result<IReadOnlyList<ListTasksForProjectResult>>>
{
    public async Task<Result<IReadOnlyList<ListTasksForProjectResult>>> HandleAsync(
        ListTasksForProjectQuery query
    )
    {
        var staff = await db
            .Tasks.Where(s => s.ProjectId == query.ProjectId)
            .OrderBy(s => s.Abbreviation)
            .Select(s => new ListTasksForProjectResult(s.Id, s.Abbreviation))
            .ToListAsync();

        return Success(staff);
    }
}
