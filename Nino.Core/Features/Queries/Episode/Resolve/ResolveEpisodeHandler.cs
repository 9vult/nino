// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episode.Resolve;

public sealed class ResolveEpisodeHandler(NinoDbContext db)
{
    public async Task<Result<EpisodeId>> HandleAsync(ResolveEpisodeQuery query)
    {
        var (projectId, number) = query;

        var episodeId = await db
            .Episodes.Where(e => e.ProjectId == projectId && e.Number == number)
            .Select(e => e.Id)
            .SingleOrDefaultAsync();

        return episodeId != default
            ? Result<EpisodeId>.Success(episodeId)
            : Result<EpisodeId>.Fail(ResultStatus.EpisodeNotFound);
    }
}
