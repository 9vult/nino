// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Domain.Entities.Conga;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Domain.ValueObjects.Abbreviation>>;

namespace Nino.Core.Features.Queries.Projects.Conga.ListFromNodeOptions;

public sealed class ListCongaFromNodeOptionsHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListCongaFromNodeOptionsQuery, Result<IReadOnlyList<Abbreviation>>>
{
    public async Task<Result<IReadOnlyList<Abbreviation>>> HandleAsync(
        ListCongaFromNodeOptionsQuery query
    )
    {
        var graph = await db
            .Projects.Where(p => p.Id == query.ProjectId)
            .Select(p => p.CongaParticipants)
            .FirstOrDefaultAsync();
        if (graph is null)
            return Fail(ResultStatus.ProjectNotFound);

        List<Abbreviation> specials = [Abbreviation.From("$AIR")];

        var groups = graph.Nodes.OfType<CongaNode.GroupNode>().Select(g => g.Name).ToList();

        var tasks = await db
            .Tasks.Where(t => t.ProjectId == query.ProjectId)
            .Select(t => t.Abbreviation)
            .Distinct()
            .ToListAsync();

        return Success([.. specials, .. groups, .. tasks]);
    }
}
