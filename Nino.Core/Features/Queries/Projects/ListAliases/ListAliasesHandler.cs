// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.QueryExtensions;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Domain.ValueObjects.Alias>>;

namespace Nino.Core.Features.Queries.Projects.ListAliases;

public sealed class ListAliasesHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListAliasesQuery, Result<IReadOnlyList<Alias>>>
{
    public async Task<Result<IReadOnlyList<Alias>>> HandleAsync(ListAliasesQuery query)
    {
        var (requestedBy, groupId, includeObservers, includeArchived) = query;

        var lookup = db
            .Projects.Where(p =>
                p.GroupId == groupId
                || (includeObservers && p.Observers.Any(o => o.GroupId == groupId))
            )
            .WhereVisibleTo(requestedBy);

        if (!includeArchived)
            lookup = lookup.Where(p => !p.IsArchived);

        var data = await lookup
            .Select(p => new
            {
                p.Id,
                p.Nickname,
                p.Aliases,
            })
            .ToListAsync();

        var results = data.SelectMany(p =>
                Enumerable.Repeat(p.Nickname, 1).Concat(p.Aliases.Select(a => a.Value))
            )
            .OrderBy(a => a.Value)
            .ToList();
        return Success(results);
    }
}
