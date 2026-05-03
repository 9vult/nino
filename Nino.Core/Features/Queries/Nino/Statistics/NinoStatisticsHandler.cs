// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;

namespace Nino.Core.Features.Queries.Nino.Statistics;

public sealed class NinoStatisticsHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<NinoStatisticsQuery, Result<NinoStatisticsResponse>>
{
    /// <inheritdoc />
    public async Task<Result<NinoStatisticsResponse>> HandleAsync(NinoStatisticsQuery query)
    {
        return Result<NinoStatisticsResponse>.Success(
            new NinoStatisticsResponse(
                TotalGroups: await db.Projects.GroupBy(p => p.GroupId).CountAsync(),
                TotalProjects: await db.Projects.CountAsync(),
                TotalEpisodes: await db.Episodes.CountAsync(),
                CompletedEpisodes: await db.Episodes.CountAsync(e => e.IsDone),
                CompletedTasks: await db.Tasks.CountAsync(t => t.IsDone),
                TotalObservers: await db.Observers.CountAsync(),
                ObserverProjectCount: await db.Observers.GroupBy(o => o.ProjectId).CountAsync()
            )
        );
    }
}
