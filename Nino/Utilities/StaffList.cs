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
        /// <param name="excludePseudo">Exclude pseudo-tasks</param>
        /// <returns>Properly-formatted roster</returns>
        public static string GenerateRoster(Project project, Episode episode, bool withWeight, bool excludePseudo = false)
        {
            StringBuilder sb = new();

            var staff = project.KeyStaff.Concat(episode.AdditionalStaff)
                .WhereIf(excludePseudo, k => !k.IsPseudo)
                .OrderBy(k => k.Role.Weight ?? 1000000);
            
            foreach (var ks in staff)
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
        /// <param name="updated">The updated task</param>
        /// <param name="excludePseudo">Exclude pseudo-tasks</param>
        /// <returns>Properly-formatted progress string</returns>
        public static string GenerateProgress(Project project, Episode episode, string? updated = null, bool excludePseudo = true)
        {
            StringBuilder sb = new();

            var staff = project.KeyStaff.Concat(episode.AdditionalStaff)
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
                else
                    if (task.Done)
                        sb.Append($"~~{task.Abbreviation}~~ ");
                    else
                        sb.Append($"**{task.Abbreviation}** ");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Generate explanatory blame progress string
        /// </summary>
        /// <param name="project">Project the episode is of</param>
        /// <param name="episode">Episode to generate the progress for</param>
        /// <param name="lng">Language code</param>
        /// <param name="updated">The updated task</param>
        /// <param name="excludePseudo">Exclude pseudo-tasks</param>
        /// <returns>Properly-formatted explanatory progress string</returns>
        public static string GenerateExplainProgress(Project project, Episode episode, string lng, string? updated = null, bool excludePseudo = true)
        {
            StringBuilder sb = new();

            var staff = project.KeyStaff.Concat(episode.AdditionalStaff)
                .WhereIf(excludePseudo, k => !k.IsPseudo)
                .OrderBy(k => k.Role.Weight ?? 1000000);
            foreach (var ks in staff)
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
