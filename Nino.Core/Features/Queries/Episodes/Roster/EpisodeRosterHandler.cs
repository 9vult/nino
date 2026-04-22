// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Services;
using Nino.Domain.Dtos;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Episodes.Roster.EpisodeRosterResponse>;

namespace Nino.Core.Features.Queries.Episodes.Roster;

public sealed class EpisodeRosterHandler(
    ReadOnlyNinoDbContext db,
    IUserVerificationService verificationService
) : IQueryHandler<EpisodeRosterQuery, Result<EpisodeRosterResponse>>
{
    /// <inheritdoc />
    public async Task<Result<EpisodeRosterResponse>> HandleAsync(EpisodeRosterQuery query)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            query.ProjectId,
            query.RequestedBy,
            PermissionsLevel.Staff
        );
        if (!verification.IsSuccess)
            return Fail(ResultStatus.Unauthorized);

        var result = await db
            .Episodes.Where(e => e.Id == query.EpisodeId)
            .Select(e => new EpisodeRosterResponse(
                e.Number,
                e.Tasks.Select(t => new EpisodeRosterStatus(
                        t.Abbreviation,
                        MappedIdDto<UserId>.From(t.Assignee),
                        t.Weight,
                        t.IsPseudo
                    ))
                    .ToList(),
                e.Project.Type == ProjectType.Movie && e.Project.Episodes.Count == 1
            ))
            .FirstOrDefaultAsync();

        return result is not null ? Success(result) : Fail(ResultStatus.EpisodeNotFound);
    }
}
