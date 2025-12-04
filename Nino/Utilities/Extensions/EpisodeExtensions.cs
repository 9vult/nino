using System.Text;
using Nino.Records;
using static Localizer.Localizer;

namespace Nino.Utilities.Extensions;

public static class EpisodeExtensions
{
    /// <summary>
    /// Verify the given user has sufficient permissions to use a command
    /// </summary>
    /// <param name="episode">Episode to verify against</param>
    /// <param name="db"></param>
    /// <param name="userId">ID of the user to check</param>
    /// <param name="excludeAdmins">Should administrators be excluded?</param>
    /// <returns>True if the user has sufficient permissions</returns>
    public static bool VerifyEpisodeUser(
        this Episode episode,
        DataContext db,
        ulong userId,
        bool excludeAdmins = false
    )
    {
        if (episode.Project.OwnerId == userId)
            return true;

        if (!excludeAdmins)
        {
            if (episode.Project.Administrators.Any(a => a.UserId == userId))
                return true;

            if (db.GetConfig(episode.GuildId)?.Administrators.Any(a => a.UserId == userId) ?? false)
                return true;
        }

        return episode.Project.KeyStaff.Any(ks => ks.UserId == userId)
            || episode.AdditionalStaff.Any(ks => ks.UserId == userId)
            || episode.PinchHitters.Any(ks => ks.UserId == userId);
    }

    /// <summary>
    /// Verify a user has sufficient permissions to make progress on a task
    /// </summary>
    /// <param name="episode">Episode to check</param>
    /// <param name="db">DataContext</param>
    /// <param name="userId">ID of the user to check</param>
    /// <param name="abbreviation">Abbreviation to check</param>
    /// <returns>True if the user has permission</returns>
    public static bool VerifyTaskUser(
        this Episode episode,
        DataContext db,
        ulong userId,
        string abbreviation
    )
    {
        if (episode.Project.OwnerId == userId)
            return true;
        if (episode.Project.Administrators.Any(a => a.UserId == userId))
            return true;
        if (db.GetConfig(episode.GuildId)?.Administrators.Any(a => a.UserId == userId) ?? false)
            return true;
        if (episode.PinchHitters.Any(ph => ph.Abbreviation == abbreviation && ph.UserId == userId))
            return true;
        if (
            episode
                .Project.KeyStaff.Concat(episode.AdditionalStaff)
                .Any(ks => ks.Role.Abbreviation == abbreviation && ks.UserId == userId)
        )
            return true;
        return false;
    }

    /// <summary>
    /// Generate an episode roster
    /// </summary>
    /// <param name="episode">Episode to generate the roster for</param>
    /// <param name="withWeight">Whether to include task weight values</param>
    /// <param name="excludePseudo">Exclude pseudo-tasks</param>
    /// <returns>Properly-formatted roster</returns>
    public static string GenerateRoster(
        this Episode episode,
        bool withWeight,
        bool excludePseudo = false
    )
    {
        StringBuilder sb = new();

        var staff = episode
            .Project.KeyStaff.Concat(episode.AdditionalStaff)
            .WhereIf(excludePseudo, k => !k.IsPseudo)
            .OrderBy(k => k.Role.Weight ?? 1000000);

        foreach (var ks in staff)
        {
            var task = episode.Tasks.First(t => t.Abbreviation == ks.Role.Abbreviation);
            var userId =
                episode
                    .PinchHitters.FirstOrDefault(k => k.Abbreviation == ks.Role.Abbreviation)
                    ?.UserId
                ?? ks.UserId;

            if (task.Done)
                sb.AppendLine(
                    $"~~{task.Abbreviation}~~: <@{userId}>{(withWeight ? $" ({ks.Role.Weight})" : string.Empty)}"
                );
            else
                sb.AppendLine(
                    $"**{task.Abbreviation}**: <@{userId}>{(withWeight ? $" ({ks.Role.Weight})" : string.Empty)}"
                );
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generate a blame progress string
    /// </summary>
    /// <param name="episode">Episode to generate the progress for</param>
    /// <param name="updated">The updated task</param>
    /// <param name="excludePseudo">Exclude pseudo-tasks</param>
    /// <returns>Properly-formatted progress string</returns>
    public static string GenerateProgress(
        this Episode episode,
        string? updated = null,
        bool excludePseudo = true
    )
    {
        StringBuilder sb = new();

        var staff = episode
            .Project.KeyStaff.Concat(episode.AdditionalStaff)
            .WhereIf(excludePseudo, k => !k.IsPseudo)
            .OrderBy(k => k.Role.Weight ?? 1000000);

        foreach (var ks in staff)
        {
            var task = episode.Tasks.First(t => t.Abbreviation == ks.Role.Abbreviation);
            if (task.Abbreviation == updated)
                if (task.Done)
                    sb.Append($"~~__{task.Abbreviation}__~~ ");
                else
                    sb.Append($"__{task.Abbreviation}__ ");
            else if (task.Done)
                sb.Append($"~~{task.Abbreviation}~~ ");
            else
                sb.Append($"**{task.Abbreviation}** ");
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Generate explanatory blame progress string
    /// </summary>
    /// <param name="episode">Episode to generate the progress for</param>
    /// <param name="lng">Language code</param>
    /// <param name="updated">The updated task</param>
    /// <param name="excludePseudo">Exclude pseudo-tasks</param>
    /// <returns>Properly-formatted explanatory progress string</returns>
    public static string GenerateExplainProgress(
        this Episode episode,
        string lng,
        string? updated = null,
        bool excludePseudo = true
    )
    {
        StringBuilder sb = new();

        var staff = episode
            .Project.KeyStaff.Concat(episode.AdditionalStaff)
            .WhereIf(excludePseudo, k => !k.IsPseudo)
            .OrderBy(k => k.Role.Weight ?? 1000000);
        foreach (var ks in staff)
        {
            var task = episode.Tasks.First(t => t.Abbreviation == ks.Role.Abbreviation);
            if (task.Abbreviation == updated)
                if (task.Done)
                    sb.AppendLine(
                        $"~~__{task.Abbreviation}__~~: {ks.Role.Name} {T("progress.explain.done", lng)}"
                    );
                else
                    sb.AppendLine(
                        $"__{task.Abbreviation}__: {ks.Role.Name} {T("progress.explain.undone", lng)}"
                    );
            else if (task.Done)
                sb.AppendLine(
                    $"~~{task.Abbreviation}~~: {ks.Role.Name} {T("progress.explain.complete", lng)}"
                );
            else
                sb.AppendLine(
                    $"**{task.Abbreviation}**: {ks.Role.Name} {T("progress.explain.incomplete", lng)}"
                );
        }

        return sb.ToString();
    }
}
