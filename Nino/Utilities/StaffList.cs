using System;
using System.Text;
using Nino.Records;

namespace Nino.Utilities
{
    internal static class StaffList
    {
        /// <summary>
        /// Generate an episode roster
        /// </summary>
        /// <param name="project">Project the episode is of</param>
        /// <param name="episode">Episode to generate the roster for</param>
        /// <returns>Properly-formatted roster</returns>
        public static string GenerateRoster(Project project, Episode episode)
        {
            StringBuilder sb = new();

            foreach (var ks in project.KeyStaff.OrderBy(k => k.Role.Weight))
            {
                Console.WriteLine(ks.Role.Weight);
                var task = episode.Tasks.First(t => t.Abbreviation == ks.Role.Abbreviation);
                if (task.Done)
                    sb.AppendLine($"~~{task.Abbreviation}~~: <@{ks.UserId}>");
                else
                    sb.AppendLine($"**{task.Abbreviation}**: <@{ks.UserId}>");
            }
            foreach (var ks in episode.AdditionalStaff)
            {
                var task = episode.Tasks.First(t => t.Abbreviation == ks.Role.Abbreviation);
                if (task.Done)
                    sb.AppendLine($"~~{task.Abbreviation}~~: <@{ks.UserId}>");
                else
                    sb.AppendLine($"**{task.Abbreviation}**: <@{ks.UserId}>");
            }

            return sb.ToString();
        }
    }
}
