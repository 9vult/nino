// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Domain.ValueObjects.TemplateStaffId>;

namespace Nino.Core.Features.Queries.TemplateStaff.Resolve;

public sealed class ResolveTemplateStaffHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ResolveTemplateStaffQuery, Result<TemplateStaffId>>
{
    public async Task<Result<TemplateStaffId>> HandleAsync(ResolveTemplateStaffQuery query)
    {
        var (projectId, abbreviation) = query;

        var staffId = await db
            .TemplateStaff.Where(t => t.ProjectId == projectId && t.Abbreviation == abbreviation)
            .Select(t => (TemplateStaffId?)t.Id)
            .FirstOrDefaultAsync();

        return staffId is not null
            ? Success(staffId.Value)
            : Fail(ResultStatus.TemplateStaffResolutionFailed);
    }
}
