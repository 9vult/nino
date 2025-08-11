using System.Diagnostics.CodeAnalysis;
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
}
