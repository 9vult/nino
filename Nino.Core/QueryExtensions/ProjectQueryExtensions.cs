// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;

namespace Nino.Core.QueryExtensions;

public static class ProjectQueryExtensions
{
    public static IQueryable<Project> WhereVisibleTo(
        this IQueryable<Project> query,
        UserId userId
    ) =>
        query.Where(p =>
            !p.IsPrivate
            || p.OwnerId == userId
            || p.Administrators.Any(a => a.UserId == userId)
            || p.Group.Configuration.Administrators.Any(a => a.UserId == userId)
            || p.TemplateStaff.Any(s => s.AssigneeId == userId)
            || p.Episodes.Any(e => e.Tasks.Any(s => s.AssigneeId == userId))
        );
}
