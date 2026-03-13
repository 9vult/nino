// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Queries.Project.Status;

public sealed class ProjectStatusHandler(NinoDbContext db)
{
    public async Task<Result<ProjectStatusResponse>> HandleAsync(ProjectStatusQuery query)
    {
        var result = await db
            .Projects.Include(p => p.Episodes)
            .Where(p => p.Id == query.ProjectId)
            .Select(p => new ProjectStatusResponse(
                p.Id,
                p.Title,
                p.Type,
                p.Episodes.Count,
                p.Episodes.Count(e => e.IsDone)
            ))
            .SingleOrDefaultAsync();

        if (result is not null)
            return Result<ProjectStatusResponse>.Success(result);
        return Result<ProjectStatusResponse>.Fail(ResultStatus.NotFound);
    }
}
