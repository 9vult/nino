// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Observers.ListNamesForProject.ListObserverNamesForProjectResult>>;

namespace Nino.Core.Features.Queries.Observers.ListNamesForProject;

public sealed class ListObserverNamesForProjectHandler(
    ReadOnlyNinoDbContext db,
    IUserVerificationService verificationService
)
    : IQueryHandler<
        ListObserverNamesForProjectQuery,
        Result<IReadOnlyList<ListObserverNamesForProjectResult>>
    >
{
    public async Task<Result<IReadOnlyList<ListObserverNamesForProjectResult>>> HandleAsync(
        ListObserverNamesForProjectQuery query
    )
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            query.ProjectId,
            query.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var observers = await db
            .Observers.Where(o => o.ProjectId == query.ProjectId)
            .Select(o => new ListObserverNamesForProjectResult(o.Id, o.Group.Name))
            .ToListAsync();

        return Success(observers);
    }
}
