// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;

namespace Nino.Core.QueryExtensions;

public static class ProjectQueryExtensions
{
    extension(IQueryable<Project> query)
    {
        public IQueryable<Project> WhereVisibleTo(UserId userId) =>
            query.Where(p =>
                !p.IsPrivate
                || p.OwnerId == userId
                || p.Tasks.Any(t => t.AssigneeId == userId)
                || p.TemplateStaff.Any(s => s.AssigneeId == userId)
                || p.Administrators.Any(a => a.UserId == userId)
                || p.Group.Configuration.Administrators.Any(a => a.UserId == userId)
            );

        public IQueryable<Project> WhereIsMember(UserId userId) =>
            query.Where(p =>
                p.OwnerId == userId
                || p.Tasks.Any(t => t.AssigneeId == userId)
                || p.TemplateStaff.Any(s => s.AssigneeId == userId)
                || p.Administrators.Any(a => a.UserId == userId)
                || p.Group.Configuration.Administrators.Any(a => a.UserId == userId)
            );
    }
}
