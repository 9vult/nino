// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.TemplateStaff.List.ListTemplateStaffResult>>;

namespace Nino.Core.Features.Queries.TemplateStaff.List;

public sealed class ListTemplateStaffHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListTemplateStaffQuery, Result<IReadOnlyList<ListTemplateStaffResult>>>
{
    public async Task<Result<IReadOnlyList<ListTemplateStaffResult>>> HandleAsync(
        ListTemplateStaffQuery query
    )
    {
        var staff = await db
            .TemplateStaff.Where(s => s.ProjectId == query.ProjectId)
            .OrderBy(s => s.Abbreviation)
            .Select(s => new ListTemplateStaffResult(s.Id, s.Abbreviation))
            .ToListAsync();

        return Success(staff);
    }
}
