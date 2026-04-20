// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Domain.Entities.Conga;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Domain.ValueObjects.Abbreviation>>;

namespace Nino.Core.Features.Queries.Projects.Conga.ListGroups;

public sealed class ListCongaGroupsHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListCongaGroupsQuery, Result<IReadOnlyList<Abbreviation>>>
{
    public async Task<Result<IReadOnlyList<Abbreviation>>> HandleAsync(ListCongaGroupsQuery query)
    {
        var graph = await db
            .Projects.Where(p => p.Id == query.ProjectId)
            .Select(p => p.CongaParticipants)
            .FirstOrDefaultAsync();
        if (graph is null)
            return Fail(ResultStatus.ProjectNotFound);

        var result = graph.Nodes.OfType<CongaNode.GroupNode>().Select(n => n.Name).ToList();
        return Success(result);
    }
}
