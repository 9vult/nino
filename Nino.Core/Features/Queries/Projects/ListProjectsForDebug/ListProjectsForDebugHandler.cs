// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Projects.ListProjectsForDebug.ListProjectsForDebugResult>>;

namespace Nino.Core.Features.Queries.Projects.ListProjectsForDebug;

public sealed class ListProjectsForDebugHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListProjectsForDebugQuery, Result<IReadOnlyList<ListProjectsForDebugResult>>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ListProjectsForDebugResult>>> HandleAsync(
        ListProjectsForDebugQuery query
    )
    {
        var user = await db
            .Users.Select(u => new { u.Id, u.IsSystemAdministrator })
            .FirstOrDefaultAsync(u => u.Id == query.RequestedBy);

        if (user is null)
            return Fail(ResultStatus.Unauthorized);

        return Success(
            await db
                .Projects.Where(p => p.GroupId == query.GroupId)
                .Where(p =>
                    user.IsSystemAdministrator
                    || p.Group.Configuration.Administrators.Any(a => a.UserId == query.RequestedBy)
                    || p.Administrators.Any(a => a.UserId == query.RequestedBy)
                )
                .Select(p => new ListProjectsForDebugResult(p.Nickname, p.Id))
                .ToListAsync()
        );
    }
}
