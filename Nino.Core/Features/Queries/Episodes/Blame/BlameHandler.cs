// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Episodes.Blame.BlameResponse>;

namespace Nino.Core.Features.Queries.Episodes.Blame;

public sealed class BlameHandler(
    ReadOnlyNinoDbContext db,
    IAniListService aniListService,
    IUserVerificationService verificationService
) : IQueryHandler<BlameQuery, Result<BlameResponse>>
{
    /// <inheritdoc />
    public async Task<Result<BlameResponse>> HandleAsync(BlameQuery query)
    {
        var includePseudo = query.IncludePseudo;
        if (includePseudo)
        {
            var verification = await verificationService.VerifyProjectPermissionsAsync(
                query.ProjectId,
                query.RequestedBy,
                PermissionsLevel.Staff
            );
            if (!verification.IsSuccess)
                includePseudo = false;
        }

        var episodeId = query.EpisodeId;
        if (episodeId is null) // Need to find the first incomplete episode
        {
            episodeId = (
                await db
                    .Episodes.Where(e => e.ProjectId == query.ProjectId)
                    .Select(e => new
                    {
                        e.Id,
                        e.Number,
                        e.IsDone,
                    })
                    .ToListAsync()
            ).OrderBy(e => e.Number.Value, StringComparer.OrdinalIgnoreCase.WithNaturalSort()).FirstOrDefault(e => !e.IsDone)?.Id;

            if (episodeId is null)
                return Fail(ResultStatus.BadRequest, "allComplete");
        }

        var result = await db
            .Episodes.Where(e => e.Id == episodeId.Value)
            .Select(e => new BlameResponse(
                e.Number,
                e.Project.AniListId,
                null,
                e.UpdatedAt,
                e.Tasks.Where(t => includePseudo && t.IsPseudo || !t.IsPseudo)
                    .Select(t => new BlameStatus(
                        t.Abbreviation,
                        t.Name,
                        t.IsDone,
                        t.Weight,
                        t.IsPseudo
                    ))
                    .ToList(),
                e.Project.Type == ProjectType.Movie && e.Project.Episodes.Count == 1
            ))
            .FirstOrDefaultAsync();

        if (result is null)
            return Fail(ResultStatus.EpisodeNotFound);

        if (!result.EpisodeNumber.IsInteger(out var number))
            return Success(result);

        var alResult = await aniListService.GetEpisodeAirTimeAsync(result.AniListId, number);
        return alResult.IsSuccess
            ? Success(result with { AiredAt = alResult.Value })
            : Success(result);
    }
}
