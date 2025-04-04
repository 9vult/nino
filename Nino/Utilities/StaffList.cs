using System.Text;
using Nino.Records;
using static Localizer.Localizer;

namespace Nino.Utilities
{
    internal static class StaffList
    {
        /// <summary>
        /// Generate an episode roster
        /// </summary>
        /// <param name="project">Project the episode is of</param>
        /// <param name="episode">Episode to generate the roster for</param>
        /// <param name="withWeight">Whether to include task weight values</param>
        /// <returns>Properly-formatted roster</returns>
        public static string GenerateRoster(Project project, Episode episode, bool withWeight)
        {
            StringBuilder sb = new();

            foreach (var ks in project.KeyStaff.Concat(episode.AdditionalStaff).OrderBy(k => k.Role.Weight ?? 1000000))
            {
                var task = episode.Tasks.First(t => t.Abbreviation == ks.Role.Abbreviation);
                var userId = episode.PinchHitters.FirstOrDefault(k => k.Abbreviation == ks.Role.Abbreviation)?.UserId ?? ks.UserId;
                
                if (task.Done)
                    sb.AppendLine($"~~{task.Abbreviation}~~: <@{userId}>{(withWeight ? $" ({ks.Role.Weight})" : string.Empty)}");
                else
                    sb.AppendLine($"**{task.Abbreviation}**: <@{userId}>{(withWeight ? $" ({ks.Role.Weight})" : string.Empty)}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate a blame progress string
        /// </summary>
        /// <param name="project">Project the episode is of</param>
        /// <param name="episode">Episode to generate the progress for</param>
        /// <returns>Properly-formatted progress string</returns>
        public static string GenerateProgress(Project project, Episode episode, string? updated = null)
        {
            StringBuilder sb = new();

            foreach (var ks in project.KeyStaff.Concat(episode.AdditionalStaff).OrderBy(k => k.Role.Weight ?? 1000000))
            {
                var task = episode.Tasks.First(t => t.Abbreviation == ks.Role.Abbreviation);
                if (task.Abbreviation == updated)
                    if (task.Done)
                        sb.Append($"~~__{task.Abbreviation}__~~ ");
                    else
                        sb.Append($"__{task.Abbreviation}__ ");
                else
                    if (task.Done)
                        sb.Append($"~~{task.Abbreviation}~~ ");
                    else
                        sb.Append($"**{task.Abbreviation}** ");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Generate an explanitory blame progress string
        /// </summary>
        /// <param name="project">Project the episode is of</param>
        /// <param name="episode">Episode to generate the progress for</param>
        /// <returns>Properly-formatted explanitory progress string</returns>
        public static string GenerateExplainProgress(Project project, Episode episode, string lng, string? updated = null)
        {
            StringBuilder sb = new();

            foreach (var ks in project.KeyStaff.Concat(episode.AdditionalStaff).OrderBy(k => k.Role.Weight ?? 1000000))
            {
                var task = episode.Tasks.First(t => t.Abbreviation == ks.Role.Abbreviation);
                if (task.Abbreviation == updated)
                    if (task.Done)
                        sb.AppendLine($"~~__{task.Abbreviation}__~~: {ks.Role.Name} {T("progress.explain.done", lng)}");
                    else
                        sb.AppendLine($"__{task.Abbreviation}__: {ks.Role.Name} {T("progress.explain.undone", lng)}");
                else
                    if (task.Done)
                        sb.AppendLine($"~~{task.Abbreviation}~~: {ks.Role.Name} {T("progress.explain.complete", lng)}");
                    else
                        sb.AppendLine($"**{task.Abbreviation}**: {ks.Role.Name} {T("progress.explain.incomplete", lng)}");
            }

            return sb.ToString();
        }
    }
}
