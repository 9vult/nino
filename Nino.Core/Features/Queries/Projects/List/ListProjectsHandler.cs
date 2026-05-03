// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Projects.List.ListProjectsResult>>;

namespace Nino.Core.Features.Queries.Projects.List;

public sealed class ListProjectsHandler(
    ReadOnlyNinoDbContext db,
    IUserVerificationService verificationService
) : IQueryHandler<ListProjectsQuery, Result<IReadOnlyList<ListProjectsResult>>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ListProjectsResult>>> HandleAsync(
        ListProjectsQuery query
    )
    {
        var verification = await verificationService.VerifyGroupPermissionsAsync(
            query.GroupId,
            query.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var results = await db
            .Projects.Where(p => p.GroupId == query.GroupId)
            .Select(p => new ListProjectsResult(
                p.Nickname,
                p.Owner.Name,
                p.IsPrivate,
                p.IsArchived,
                p.Episodes.Count,
                p.Observers.Count,
                p.DelegateObserverId.HasValue,
                p.AirNotificationsEnabled,
                p.CongaRemindersEnabled,
                p.Tasks.Count,
                p.Tasks.Count(t => t.IsDone),
                p.Tasks.Count(t => !t.IsPseudo),
                p.Tasks.Count(t => !t.IsPseudo && t.IsDone)
            ))
            .ToListAsync();
        return Success(results);
    }
}
