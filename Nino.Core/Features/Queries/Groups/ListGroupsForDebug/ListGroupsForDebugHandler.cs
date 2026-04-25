// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Groups.ListGroupsForDebug.ListGroupsForDebugResult>>;

namespace Nino.Core.Features.Queries.Groups.ListGroupsForDebug;

public sealed class ListGroupsForDebugHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListGroupsForDebugQuery, Result<IReadOnlyList<ListGroupsForDebugResult>>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ListGroupsForDebugResult>>> HandleAsync(
        ListGroupsForDebugQuery query
    )
    {
        var user = await db
            .Users.Select(u => new { u.Id, u.IsSystemAdministrator })
            .FirstOrDefaultAsync(u => u.Id == query.RequestedBy);

        if (user is null)
            return Fail(ResultStatus.Unauthorized);

        return Success(
            await db
                .Groups.Where(g =>
                    user.IsSystemAdministrator
                    || g.Configuration.Administrators.Any(a => a.UserId == user.Id)
                    || g.Projects.Any(p => p.Administrators.Any(a => a.UserId == user.Id))
                )
                .Select(g => new ListGroupsForDebugResult(g.Name, g.Id))
                .ToListAsync()
        );
    }
}
