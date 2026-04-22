// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Episodes.BlameAll.BlameAllResponse>;

namespace Nino.Core.Features.Queries.Episodes.BlameAll;

public sealed class BlameAllHandler(
    ReadOnlyNinoDbContext db,
    IAniListService aniListService,
    IUserVerificationService verificationService
) : IQueryHandler<BlameAllQuery, Result<BlameAllResponse>>
{
    /// <inheritdoc />
    public async Task<Result<BlameAllResponse>> HandleAsync(BlameAllQuery query)
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

        var project = await db
            .Projects.Include(p => p.Episodes)
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        var episodes = project
            .Episodes.Where(e =>
                query.Filter switch
                {
                    BlameAllFilter.Incomplete => !e.IsDone,
                    BlameAllFilter.InProgress => !e.IsDone && e.Tasks.Any(t => t.IsDone),
                    _ => true,
                }
            )
            .OrderBy(e => e.Number.Value, StringComparer.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        // Calculate pages
        // Thanks to petzku and astiob for their contributions to this algorithm
        var pageCount = Math.Ceiling(episodes.Count / 13d);
        var pageLength = Math.Floor(episodes.Count / pageCount);
        var roundUp = episodes.Count % pageCount;

        var firstUndoneIdx = episodes.TakeWhile(e => e.IsDone).Count();
        if (firstUndoneIdx >= episodes.Count)
            firstUndoneIdx = 0;
        var startPage = query.Page ?? Math.Max(Math.Ceiling(firstUndoneIdx / pageLength) - 1, 0);

        var skip = (int)(startPage * pageLength + Math.Min(startPage, roundUp));
        var length = (int)(pageLength + (startPage + 1 <= roundUp ? 1 : 0));

        var pagedEpisodes = episodes.Skip(skip).Take(length);

        List<BlameAllEpisodeStatus> episodeStatuses = [];
        foreach (var episode in pagedEpisodes)
        {
            var status = new BlameAllEpisodeStatus(
                episode.Number,
                null,
                episode.UpdatedAt,
                episode
                    .Tasks.Where(t => includePseudo && t.IsPseudo || !t.IsPseudo)
                    .Select(t => new BlameAllTaskStatus(
                        t.Abbreviation,
                        t.Name,
                        t.IsDone,
                        t.Weight,
                        t.IsPseudo
                    ))
                    .ToList()
            );
            if (episode.IsDone || !episode.Number.IsInteger(out var number))
            {
                episodeStatuses.Add(status);
                continue;
            }
            var alResult = await aniListService.GetEpisodeAirTimeAsync(project.AniListId, number);
            if (alResult.IsSuccess)
                episodeStatuses.Add(status with { AiredAt = alResult.Value });
            else
                episodeStatuses.Add(status);
        }

        return Success(new BlameAllResponse(episodeStatuses, (int)startPage, (int)pageCount));
    }
}
