using System.Text;
using Nino.Records;
using Nino.Records.Enums;

namespace Nino.Utilities;

public static class CongaHelper
{
    /// <summary>
    /// Get a list of all currently-tardy tasks for an episode
    /// </summary>
    /// <param name="project">Project the episode is from</param>
    /// <param name="episode">The episode to check</param>
    /// <param name="checkDate">Check if the task needs to be reminded</param>
    /// <returns>A list of tardy task abbreviations</returns>
    public static List<string> GetTardyTasks(
        Project project,
        Episode episode,
        bool checkDate = true
    )
    {
        var taskLookup = episode.Tasks.ToDictionary(t => t.Abbreviation, t => t);
        var graph = project.CongaParticipants;
        var nextTasks = new List<string>();

        foreach (var nextTask in taskLookup.Keys)
        {
            var prerequisites = graph.GetPrerequisitesFor(nextTask).ToList();
            if (prerequisites.Count == 0)
                continue;

            // Check if all prereqs are complete and that the task exists
            if (
                !prerequisites.All(p =>
                    p.Type == CongaNodeType.Special
                    || (taskLookup.TryGetValue(p.Abbreviation, out var pTask) && pTask.Done)
                )
            )
                continue;
            if (!taskLookup.TryGetValue(nextTask, out var task) || task.Done)
                continue;

            // We aren't checking the date here
            if (!checkDate)
            {
                nextTasks.Add(nextTask);
                continue;
            }

            // Add to the list if the task is indeed tardy
            if (
                !taskLookup.TryGetValue(nextTask, out var candidate)
                || candidate.LastReminded is null
            )
                continue;
            if (
                candidate.LastReminded?.AddMinutes(-2)
                < DateTimeOffset.UtcNow - project.CongaReminderPeriod
            )
                nextTasks.Add(nextTask);
        }

        return nextTasks;
    }

    /// <summary>
    /// Get an url-encoded dot graph format string for generating an image of the graph
    /// </summary>
    /// <param name="project">Project to generate the graph of</param>
    /// <param name="forceAdditional">Force inclusion of additional staff</param>
    /// <returns>Url-encoded string</returns>
    public static string GetDot(Project project, bool forceAdditional = false)
    {
        var edges = new HashSet<string>();
        var sb = new StringBuilder();
        sb.AppendLine("digraph {");
        sb.AppendLine("rankdir=LR;");

        var nodes = forceAdditional
            ? project.CongaParticipants.Nodes.ToList()
            : project
                .CongaParticipants.Nodes.Where(n => n.Type != CongaNodeType.AdditionalStaff)
                .ToList();

        var groups = nodes
            .Where(n => n.Type is CongaNodeType.Group)
            .OrderBy(g => g.Abbreviation)
            .ToList();
        var entries = nodes
            .Where(n => n.Type is not CongaNodeType.Group)
            .OrderBy(n => n.Abbreviation)
            .ToList();

        // Add groups
        foreach (var group in groups)
        {
            var members = group
                .Dependents.Where(d => d.Dependents.Contains(group))
                .Where(d => d.Type != CongaNodeType.Group)
                .ToList();

            if (members.Count == 0)
                continue;

            sb.AppendLine($"subgraph cluster{Sanitize(group.Abbreviation)} {{");
            sb.AppendLine($"label=\"{group.Abbreviation}\"");
            sb.AppendLine("style=rounded; color=grey;");

            foreach (var member in members)
                sb.AppendLine($"{Sanitize(member.Abbreviation)};");
            sb.AppendLine("}");
        }

        // Add entries
        foreach (var node in entries)
        {
            var isPseudo =
                project
                    .KeyStaff.FirstOrDefault(t => t.Role.Abbreviation == node.Abbreviation)
                    ?.IsPseudo
                ?? false;
            var shape =
                node.Type == CongaNodeType.Special ? "diamond"
                : isPseudo ? "Mcircle"
                : "circle";
            var style = node.Type == CongaNodeType.AdditionalStaff ? "filled,dashed" : "filled";

            sb.AppendLine(
                $"\"{Sanitize(node.Abbreviation)}\" [label=\"{node.Abbreviation}\", style=\"{style}\", fillcolor=white, shape={shape}, width=0.75];"
            );
        }

        // Add edges
        foreach (var node in nodes)
        {
            var dependencies = node.Type is not CongaNodeType.Group
                ? node.Dependents
                : node.Dependents.Where(d => !d.Dependents.Contains(node));

            foreach (var dep in dependencies)
            {
                if (node.Prerequisites.Contains(dep))
                    continue;

                // Entry -> Entry
                if (node.Type is not CongaNodeType.Group && dep.Type is not CongaNodeType.Group)
                {
                    CreateEdge(Sanitize(node.Abbreviation), Sanitize(dep.Abbreviation));
                }
                // Entry -> Group ---> Entry -> Member
                else if (node.Type is not CongaNodeType.Group && dep.Type is CongaNodeType.Group)
                {
                    var members = dep
                        .Dependents.Where(m => m.Dependents.Contains(dep))
                        .Where(m => m.Type != CongaNodeType.Group);

                    foreach (var m in members)
                        CreateEdge(Sanitize(node.Abbreviation), Sanitize(m.Abbreviation));
                }
                // Group -> Entry ---> Member -> Entry
                else if (node.Type is CongaNodeType.Group && dep.Type is not CongaNodeType.Group)
                {
                    var members = node
                        .Dependents.Where(m => m.Dependents.Contains(node))
                        .Where(m => m.Type != CongaNodeType.Group);

                    foreach (var m in members)
                        CreateEdge(Sanitize(m.Abbreviation), Sanitize(dep.Abbreviation));
                }
            }
        }

        sb.Append('}');
        return sb.ToString();

        void CreateEdge(string from, string to)
        {
            var key = $"{from} -> {to};";
            if (edges.Add(key))
                sb.AppendLine(key);
        }
    }

    /// <summary>
    /// Get an url-encoded dot graph format string for generating an image of the graph
    /// </summary>
    /// <param name="project">Project to generate the graph of</param>
    /// <param name="episode">Episode to generate the graph for</param>
    /// <param name="forceAdditional">Force inclusion of additional staff</param>
    /// <returns>Url-encoded string</returns>
    public static string GetDot(Project project, Episode episode, bool forceAdditional = false)
    {
        var edges = new HashSet<string>();
        var sb = new StringBuilder();
        sb.AppendLine("digraph {");
        sb.AppendLine("rankdir=LR;");

        var nodes = forceAdditional
            ? project.CongaParticipants.Nodes.ToList()
            : project
                .CongaParticipants.Nodes.Where(n =>
                    n.Type != CongaNodeType.AdditionalStaff
                    || episode.Tasks.Any(t => t.Abbreviation == n.Abbreviation)
                )
                .ToList();

        var groups = nodes
            .Where(n => n.Type is CongaNodeType.Group)
            .OrderBy(g => g.Abbreviation)
            .ToList();
        var entries = nodes
            .Where(n => n.Type is not CongaNodeType.Group)
            .OrderBy(n => n.Abbreviation)
            .ToList();

        // Add groups
        foreach (var group in groups)
        {
            var members = group
                .Dependents.Where(d => d.Dependents.Contains(group))
                .Where(d => d.Type != CongaNodeType.Group)
                .ToList();

            if (members.Count == 0)
                continue;

            sb.AppendLine($"subgraph cluster{Sanitize(group.Abbreviation)} {{");
            sb.AppendLine($"label=\"{group.Abbreviation}\"");
            sb.AppendLine("style=rounded; color=grey;");

            foreach (var member in members)
                sb.AppendLine($"{Sanitize(member.Abbreviation)};");
            sb.AppendLine("}");
        }

        // Add entries
        foreach (var node in entries)
        {
            var task = episode.Tasks.FirstOrDefault(t => t.Abbreviation == node.Abbreviation);
            var isPseudo =
                project
                    .KeyStaff.Concat(episode.AdditionalStaff)
                    .FirstOrDefault(t => t.Role.Abbreviation == node.Abbreviation)
                    ?.IsPseudo
                ?? false;
            var color =
                node.Type == CongaNodeType.Special ? "yellow"
                : task is null ? "red"
                : task.Done ? "green"
                : "orange";
            var shape =
                node.Type == CongaNodeType.Special ? "diamond"
                : isPseudo ? "Mcircle"
                : "circle";
            var style = node.Type == CongaNodeType.AdditionalStaff ? "filled,dashed" : "filled";

            sb.AppendLine(
                $"\"{Sanitize(node.Abbreviation)}\" [label=\"{node.Abbreviation}\", style=\"{style}\", fillcolor={color}, shape={shape}, width=0.75];"
            );
        }

        // Add edges
        foreach (var node in nodes)
        {
            var dependencies = node.Type is not CongaNodeType.Group
                ? node.Dependents
                : node.Dependents.Where(d => !d.Dependents.Contains(node));

            foreach (var dep in dependencies)
            {
                if (node.Prerequisites.Contains(dep))
                    continue;

                // Entry -> Entry
                if (node.Type is not CongaNodeType.Group && dep.Type is not CongaNodeType.Group)
                {
                    CreateEdge(Sanitize(node.Abbreviation), Sanitize(dep.Abbreviation));
                }
                // Entry -> Group ---> Entry -> Member
                else if (node.Type is not CongaNodeType.Group && dep.Type is CongaNodeType.Group)
                {
                    var members = dep
                        .Dependents.Where(m => m.Dependents.Contains(dep))
                        .Where(m => m.Type != CongaNodeType.Group);

                    foreach (var m in members)
                        CreateEdge(Sanitize(node.Abbreviation), Sanitize(m.Abbreviation));
                }
                // Group -> Entry ---> Member -> Entry
                else if (node.Type is CongaNodeType.Group && dep.Type is not CongaNodeType.Group)
                {
                    var members = node
                        .Dependents.Where(m => m.Dependents.Contains(node))
                        .Where(m => m.Type != CongaNodeType.Group);

                    foreach (var m in members)
                        CreateEdge(Sanitize(m.Abbreviation), Sanitize(dep.Abbreviation));
                }
            }
        }

        sb.Append('}');
        return sb.ToString();

        void CreateEdge(string from, string to)
        {
            var key = $"{from} -> {to};";
            if (edges.Add(key))
                sb.AppendLine(key);
        }
    }

    /// <summary>
    /// Sanitize inputs to be usable for DOT notation
    /// </summary>
    /// <param name="input">User input</param>
    /// <returns>DOT-sanitized output</returns>
    private static string Sanitize(string input)
    {
        return "n_" + string.Concat(input.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
    }
}
