// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Staff.Resolve;

public sealed class ResolveStaffHandler(NinoDbContext db)
{
    public async Task<Result<StaffId>> ResolveAsync(ResolveKeyStaffQuery query)
    {
        var (projectId, abbreviation) = query;

        var taskId = await db
            .Staff.Where(t => t.ProjectId == projectId && t.Role.Abbreviation == abbreviation)
            .Select(e => e.Id)
            .SingleOrDefaultAsync();

        return projectId != default
            ? Result<StaffId>.Success(taskId)
            : Result<StaffId>.Fail(ResultStatus.StaffNotFound);
    }

    public async Task<Result<StaffId>> ResolveAsync(ResolveAdditionalStaffQuery query)
    {
        var (episodeId, abbreviation) = query;

        var taskId = await db
            .Staff.Where(t => t.EpisodeId == episodeId && t.Role.Abbreviation == abbreviation)
            .Select(e => e.Id)
            .SingleOrDefaultAsync();

        return episodeId != default
            ? Result<StaffId>.Success(taskId)
            : Result<StaffId>.Fail(ResultStatus.StaffNotFound);
    }
}
