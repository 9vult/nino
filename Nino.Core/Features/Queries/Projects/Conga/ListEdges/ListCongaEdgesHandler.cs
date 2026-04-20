// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Projects.Conga.ListEdges.ListCongaEdgesResult>>;

namespace Nino.Core.Features.Queries.Projects.Conga.ListEdges;

public sealed class ListCongaEdgesHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ListCongaEdgesQuery, Result<IReadOnlyList<ListCongaEdgesResult>>>
{
    public async Task<Result<IReadOnlyList<ListCongaEdgesResult>>> HandleAsync(
        ListCongaEdgesQuery query
    )
    {
        var graph = await db
            .Projects.Where(p => p.Id == query.ProjectId)
            .Select(p => p.CongaParticipants)
            .FirstOrDefaultAsync();
        if (graph is null)
            return Fail(ResultStatus.ProjectNotFound);

        var result = graph
            .Nodes.SelectMany(n =>
                n.Dependents.Select(dep => new ListCongaEdgesResult(n.Name, dep.Name))
            )
            .ToList();

        return Success(result);
    }
}
