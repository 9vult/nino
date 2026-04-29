// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Observers.List.ListObserversResult>>;

namespace Nino.Core.Features.Queries.Observers.List;

public sealed class ListObserversHandler(
    ReadOnlyNinoDbContext db,
    IUserVerificationService verificationService
) : IQueryHandler<ListObserversQuery, Result<IReadOnlyList<ListObserversResult>>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ListObserversResult>>> HandleAsync(
        ListObserversQuery query
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
            .Observers.Where(p => p.GroupId == query.GroupId)
            .Select(p => new ListObserversResult(
                p.Project.Nickname,
                p.Owner.Name,
                p.Project.DelegateObserverId == p.Id
            ))
            .ToListAsync();
        return Success(results);
    }
}
