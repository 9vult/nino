using System.Diagnostics.CodeAnalysis;
using System.Text;
using NaturalSort.Extension;
using Nino.Records;

namespace Nino.Utilities.Extensions;

public static class ProjectExtensions
{
    /// <summary>
    /// Try to get an episode
    /// </summary>
    /// <param name="project">Project to get the episode from</param>
    /// <param name="number">Number of the episode</param>
    /// <param name="episode">Episode</param>
    /// <returns>Found episode, or <see langword="null"/> if it doesn't exist</returns>
    public static bool TryGetEpisode(
        this Project project,
        string number,
        [NotNullWhen(true)] out Episode? episode
    )
    {
        episode = project.Episodes.FirstOrDefault(e => e.Number == number);
        return episode is not null;
    }

    /// <summary>
    /// Verify the given user has sufficient permissions to use a command
    /// </summary>
    /// <param name="project">Project to verify against</param>
    /// <param name="db">DataContext</param>
    /// <param name="userId">ID of the user to check</param>
    /// <param name="excludeAdmins">Should administrators be excluded?</param>
    /// <param name="includeStaff">Should Staff be included?</param>
    /// <returns>True if the user has sufficient permissions</returns>
    public static bool VerifyUser(
        this Project project,
        DataContext db,
        ulong userId,
        bool excludeAdmins = false,
        bool includeStaff = false
    )
    {
        if (project.OwnerId == userId)
            return true;

        if (!excludeAdmins)
        {
            if (project.Administrators.Any(a => a.UserId == userId))
                return true;

            if (db.GetConfig(project.GuildId)?.Administrators.Any(a => a.UserId == userId) ?? false)
                return true;
        }

        if (!includeStaff)
            return false;

        return project.KeyStaff.Any(s => s.UserId == userId)
            || project.Episodes.Any(e =>
                e.AdditionalStaff.Any(s => s.UserId == userId)
                || e.PinchHitters.Any(p => p.UserId == userId)
            );
    }

    public static string GenerateRoster(this Project project, bool excludePseudo = true)
    {
        // Get a list of every task and who performed it
        List<TaskInfo> tasks = [];
        foreach (var episode in project.Episodes)
        {
            var staff = episode.AdditionalStaff.Concat(project.KeyStaff).ToList();
            foreach (var task in episode.Tasks)
            {
                var position = staff.FirstOrDefault(s => s.Role.Abbreviation == task.Abbreviation);
                if (position is null || (position.IsPseudo && excludePseudo))
                    continue;

                var pinch = episode.PinchHitters.FirstOrDefault(ph =>
                    ph.Abbreviation == task.Abbreviation
                );

                tasks.Add(
                    new TaskInfo
                    {
                        Name = position.Role.Name,
                        Episode = episode.Number,
                        UserId = pinch?.UserId ?? position.UserId,
                        Weight = position.Role.Weight ?? 0,
                    }
                );
            }
        }

        // Grouping
        var groups = tasks
            .OrderBy(t => t.Weight)
            .GroupBy(t => t.Name)
            .Select(taskGroup => new
            {
                Name = taskGroup.Key,
                Assignees = taskGroup
                    .GroupBy(a => a.UserId)
                    .Select(assigneeGroup => new
                    {
                        AssigneeId = assigneeGroup.Key,
                        Episodes = string.Join(
                            ", ",
                            ToRanges(assigneeGroup.Select(x => x.Episode))
                        ),
                    }),
            });

        var sb = new StringBuilder();
        foreach (var group in groups)
        {
            sb.Append($"**{group.Name}**: ");
            sb.AppendLine(
                string.Join(", ", group.Assignees.Select(a => $"<@{a.AssigneeId}> ({a.Episodes})"))
            );
        }

        return sb.ToString();
    }

    private static IEnumerable<string> ToRanges(IEnumerable<string> numbers)
    {
        var sorted = numbers
            .OrderBy(n => n, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();
        if (sorted.Count == 0)
            yield break;

        var start = sorted[0];
        var end = start;
        for (var i = 1; i < sorted.Count; i++)
        {
            if (
                Episode.EpisodeNumberIsInteger(sorted[i], out var cur)
                && Episode.EpisodeNumberIsInteger(end, out var next)
                && cur == next + 1
            )
            {
                end = sorted[i];
            }
            else
            {
                yield return FormatRange(start, end);
                start = end = sorted[i];
            }
        }
        yield return FormatRange(start, end);
    }

    private static string FormatRange(string start, string end) =>
        start == end ? start : $"{start}-{end}";

    private class TaskInfo
    {
        public required string Name { get; set; }
        public required ulong UserId { get; set; }
        public required string Episode { get; set; }
        public required decimal Weight { get; set; }
    }
}
