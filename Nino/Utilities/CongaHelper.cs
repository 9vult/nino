using System.Text;
using Nino.Records;

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
    public static List<string> GetTardyTasks(Project project, Episode episode, bool checkDate = true)
    {
        var taskLookup = episode.Tasks.ToDictionary(t => t.Abbreviation, t => t);
        var graph = project.CongaParticipants;
        var nextTasks = new List<string>();
        
        foreach (var nextTask in taskLookup.Keys)
        {
            var prerequisites = graph.GetPrerequisitesFor(nextTask).ToList();
            if (prerequisites.Count == 0) continue;
            
            // Check if all prereqs are complete and that the task exists
            if (!prerequisites.All(p => taskLookup.TryGetValue(p.Abbreviation, out var pTask) && pTask.Done)) continue;
            if (!taskLookup.TryGetValue(nextTask, out var task) || task.Done) continue;
            
            // We aren't checking the date here
            if (!checkDate)
            {
                nextTasks.Add(nextTask);
                continue;
            }
            
            // Add to the list if the task is indeed tardy
            if (!taskLookup.TryGetValue(nextTask, out var candidate) || candidate.LastReminded is null) continue;
            if (candidate.LastReminded?.AddMinutes(-2) < DateTimeOffset.UtcNow - project.CongaReminderPeriod)
                nextTasks.Add(nextTask);
        }
        
        return nextTasks;
    }
    
    /// <summary>
    /// Get an url-encoded dot graph format string for generating an image of the graph
    /// </summary>
    /// <param name="project">Project to generate the graph of</param>
    /// <returns>Url-encoded string</returns>
    public static string GetDot(Project project)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph {");
        
        foreach (var node in project.CongaParticipants.Nodes)
        {
            sb.AppendLine($"    \"{node.Abbreviation}\";");

            foreach (var dependent in node.Dependents)
            {
                sb.AppendLine($"    \"{node.Abbreviation}\" -> \"{dependent.Abbreviation}\";");
            }
        }

        sb.AppendLine("}");
        return Uri.EscapeDataString(sb.ToString());
    }

    /// <summary>
    /// Get an url-encoded dot graph format string for generating an image of the graph
    /// </summary>
    /// <param name="project">Project to generate the graph of</param>
    /// <param name="episode">Episode to generate the graph for</param>
    /// <returns>Url-encoded string</returns>
    public static string GetDot(Project project, Episode episode)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph {");
        sb.AppendLine("    node [style=filled];");

        foreach (var node in project.CongaParticipants.Nodes)
        {
            var task = episode.Tasks.FirstOrDefault(t => t.Abbreviation == node.Abbreviation);
            var color = task is null ? "red" : task.Done ? "green" : "orange";
            
            sb.AppendLine($"    \"{node.Abbreviation}\" [fillcolor={color}];");

            foreach (var dependent in node.Dependents)
            {
                sb.AppendLine($"    \"{node.Abbreviation}\" -> \"{dependent.Abbreviation}\";");
            }
        }
        
        sb.AppendLine("}");
        return Uri.EscapeDataString(sb.ToString());
    }
}