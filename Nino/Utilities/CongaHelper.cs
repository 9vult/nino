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
        var sb = new StringBuilder();
        sb.Append("digraph {");

        var nodes = forceAdditional
            ? project.CongaParticipants.Nodes
            : project.CongaParticipants.Nodes.Where(n => n.Type != CongaNodeType.AdditionalStaff);

        foreach (var node in nodes.OrderBy(n => n.Abbreviation))
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

            sb.Append(
                $"\"{node.Abbreviation}\" [style=\"{style}\" fillcolor=white, shape={shape}, width=0.75];"
            );

            var dependents = forceAdditional
                ? node.Dependents
                : node.Dependents.Where(n => n.Type != CongaNodeType.AdditionalStaff);

            foreach (var dependent in dependents)
            {
                sb.AppendLine($"\"{node.Abbreviation}\" -> \"{dependent.Abbreviation}\";");
            }
        }

        sb.Append('}');
        return sb.ToString();
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
        var sb = new StringBuilder();
        sb.Append("digraph {");

        // Filter nodes

        var nodes = forceAdditional
            ? project.CongaParticipants.Nodes
            : project.CongaParticipants.Nodes.Where(n =>
                n.Type != CongaNodeType.AdditionalStaff
                || episode.Tasks.Any(t => t.Abbreviation == n.Abbreviation)
            );

        foreach (var node in nodes.OrderBy(n => n.Abbreviation))
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

            sb.Append(
                $"\"{node.Abbreviation}\" [style=\"{style}\" fillcolor={color}, shape={shape}, width=0.75];"
            );

            var dependents = forceAdditional
                ? node.Dependents
                : node.Dependents.Where(n =>
                    n.Type != CongaNodeType.AdditionalStaff
                    || episode.Tasks.Any(t => t.Abbreviation == n.Abbreviation)
                );

            foreach (var dependent in dependents)
            {
                sb.Append($"\"{node.Abbreviation}\" -> \"{dependent.Abbreviation}\";");
            }
        }

        sb.Append('}');
        return sb.ToString();
    }
}
