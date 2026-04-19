// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Domain.ValueObjects.EpisodeId>;

namespace Nino.Core.Features.Queries.Episodes.Resolve;

public sealed class ResolveEpisodeHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ResolveEpisodeQuery, Result<EpisodeId>>
{
    public async Task<Result<EpisodeId>> HandleAsync(ResolveEpisodeQuery query)
    {
        var (projectId, number) = query;

        var episodeId = await db
            .Episodes.Where(e => e.ProjectId == projectId && e.Number == number)
            .Select(e => (EpisodeId?)e.Id)
            .FirstOrDefaultAsync();

        return episodeId is not null
            ? Success(episodeId.Value)
            : Fail(ResultStatus.EpisodeResolutionFailed);
    }
}
