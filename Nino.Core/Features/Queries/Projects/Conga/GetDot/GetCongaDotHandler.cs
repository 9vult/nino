// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Microsoft.EntityFrameworkCore;
using Nino.Domain.Entities.Conga;
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
            b.AppendLine($"subgraph \"{group.Name}\" {{");
            b.AppendLine($"label=\"{group.Name}\"");
            b.AppendLine("style=rounded");
            b.AppendLine("bgcolor=\"#f5f5f5\"");
            b.AppendLine("pencolor=\"#aaaaaa\"");
            b.AppendLine("margin=12");

            // Add anchor node
            b.AppendLine($""" "{group.Name}_anchor" [style=invis width=0 height=0 label=""]""");

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
                            $""" "{from.Name}_anchor" -> "{to.Name}" [ltail="{from.Name}"]""",
                        (false, true) =>
                            $""" "{from.Name}" -> "{to.Name}_anchor" [lhead="{to.Name}"]""",
                        (true, true) =>
                            $""" "{from.Name}_anchor" -> "{to.Name}_anchor" [ltail="{from.Name}",lhead="{to.Name}"]""",
                    }
                );
            }
        }

        b.AppendLine("}");
        return Success(b.ToString());
    }
}
