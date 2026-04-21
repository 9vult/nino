// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Microsoft.EntityFrameworkCore;
using Nino.Domain.Entities.Conga;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<string>;

namespace Nino.Core.Features.Queries.Projects.Conga.GetDot;

public sealed class GetCongaDotHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetCongaDotQuery, Result<string>>
{
    /// <inheritdoc />
    public async Task<Result<string>> HandleAsync(GetCongaDotQuery query)
    {
        var graph = await db
            .Projects.Where(p => p.Id == query.ProjectId)
            .Select(p => p.CongaParticipants)
            .FirstOrDefaultAsync();
        if (graph is null)
            return Fail(ResultStatus.ProjectNotFound);

        if (graph.Nodes.Count == 0)
            return Success(string.Empty);

        var tasks = query.EpisodeId.HasValue
            ? await db.Tasks.Where(t => t.EpisodeId == query.EpisodeId.Value).ToListAsync()
            : await db.Tasks.Where(t => t.ProjectId == query.ProjectId).ToListAsync();

        Dictionary<Abbreviation, (string, string)> groupTargets = [];

        var b = new StringBuilder();
        b.AppendLine("digraph conga {");
        b.AppendLine("compound=true");
        b.AppendLine("rankdir=LR");
        b.AppendLine("node [shape=circle fixedsize=false width=0.8]");

        // Add root nodes
        foreach (var node in graph.Children.OfType<CongaNode.TaskNode>())
        {
            var task = tasks.FirstOrDefault(t => t.Abbreviation == node.Name);

            var style = task?.IsPseudo ?? false ? "filled,dashed" : "filled";
            var color = "white";
            if (query.EpisodeId.HasValue)
            {
                if (node.Name.Value.StartsWith('$'))
                    color = "yellow";
                else if (task?.IsDone ?? true)
                    color = "green";
                else
                    color = "orange";
            }

            b.AppendLine(
                $""" "{node.Name}" [label="{node.Name}",style="{style}",fillcolor="{color}"]"""
            );
        }

        // Add groups
        foreach (var group in graph.Children.OfType<CongaNode.GroupNode>())
        {
            b.AppendLine($"subgraph \"cluster_{group.Name}\" {{");
            b.AppendLine($"label=\"{group.Name}\"");
            b.AppendLine("style=rounded");
            b.AppendLine("bgcolor=\"#f5f5f5\"");
            b.AppendLine("pencolor=\"#aaaaaa\"");

            // If no children, add an anchor node and continue
            if (group.Children.Count == 0)
            {
                b.AppendLine($""" "{group.Name}_anchor" [style=invis width=0 height=0 label=""]""");
                groupTargets[group.Name] = ($"{group.Name}_anchor", $"{group.Name}_anchor");
                b.AppendLine("}");
                continue;
            }

            // Add group children
            foreach (var node in group.Children.OfType<CongaNode.TaskNode>())
            {
                var task = tasks.FirstOrDefault(t => t.Abbreviation == node.Name);

                var style = task?.IsPseudo ?? false ? "filled,dashed" : "filled";
                var color = "white";
                if (query.EpisodeId.HasValue)
                {
                    if (node.Name.Value.StartsWith('$'))
                        color = "yellow";
                    else if (task?.IsDone ?? true)
                        color = "green";
                    else
                        color = "orange";
                }

                b.AppendLine(
                    $""" "{node.Name}" [label="{node.Name}",style="{style}",fillcolor="{color}"]"""
                );
            }

            // Add inner-group edges
            foreach (var from in group.Children)
            {
                foreach (var to in from.Dependents)
                {
                    b.AppendLine($"\"{from.Name}\" -> \"{to.Name}\"");
                }
            }

            // Try to determine arrow targets
            var roots = group.Children.Where(n => n.IsRootNode).ToList();
            var inTarget = roots[roots.Count / 2]; // middle

            var longest = -1;
            var outTarget = inTarget;
            foreach (var node in roots)
            {
                if (CongaNode.GetSubtree(node).Count <= longest)
                    continue;
                longest = CongaNode.GetSubtree(node).Count;
                outTarget = CongaNode.GetFurthestLeaf(node);
            }
            groupTargets[group.Name] = (inTarget.Name.Value, outTarget.Name.Value);

            b.AppendLine("}");
        }

        // Add root edges
        foreach (var from in graph.Children)
        {
            foreach (var to in from.Dependents)
            {
                b.AppendLine(
                    (from is CongaNode.GroupNode, to is CongaNode.GroupNode) switch
                    {
                        (false, false) => $""" "{from.Name}" -> "{to.Name}" """,
                        (true, false) =>
                            $""" "{groupTargets[from.Name].Item2}" -> "{to.Name}" [ltail="cluster_{from.Name}"]""",
                        (false, true) =>
                            $""" "{from.Name}" -> "{groupTargets[to.Name].Item1}" [lhead="cluster_{to.Name}"]""",
                        (true, true) =>
                            $""" "{groupTargets[from.Name].Item2}" -> "{groupTargets[to.Name].Item1}" [ltail="cluster_{from.Name}",lhead="cluster_{to.Name}"]""",
                    }
                );
            }
        }

        b.AppendLine("}");
        return Success(b.ToString());
    }
}
