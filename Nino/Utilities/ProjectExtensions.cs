using System.Diagnostics.CodeAnalysis;
using Nino.Records;

namespace Nino.Utilities;

public static class ProjectExtensions
{
    /// <summary>
    /// Try to get an episode
    /// </summary>
    /// <param name="project">Project to get the episode from</param>
    /// <param name="number">Number of the episode</param>
    /// <param name="episode">Episode</param>
    /// <returns>Found episode, or <see langword="null"/> if it doesn't exist</returns>
    public static bool TryGetEpisode(this Project project, string number, [NotNullWhen(true)] out Episode? episode)
    {
        episode = project.Episodes.FirstOrDefault(e => e.Number == number);
        return episode is not null;
    }
}