// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Observers.ListNamesForGroup.ListObserverNamesForGroupResult>>;

namespace Nino.Core.Features.Queries.Observers.ListNamesForGroup;

public sealed class ListObserverNamesForGroupHandler(
    ReadOnlyNinoDbContext db,
    IUserVerificationService verificationService
)
    : IQueryHandler<
        ListObserverNamesForGroupQuery,
        Result<IReadOnlyList<ListObserverNamesForGroupResult>>
    >
{
    public async Task<Result<IReadOnlyList<ListObserverNamesForGroupResult>>> HandleAsync(
        ListObserverNamesForGroupQuery query
    )
    {
        var verification = await verificationService.VerifyGroupPermissionsAsync(
            query.GroupId,
            query.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess && !query.OverrideVerification)
            return Fail(verification.Status);

        var observers = await db
            .Observers.Where(o => o.GroupId == query.GroupId)
            .Select(o => new ListObserverNamesForGroupResult(
                o.Id,
                o.Group.Name,
                o.Project.Nickname
            ))
            .ToListAsync();

        return Success(observers);
    }
}
