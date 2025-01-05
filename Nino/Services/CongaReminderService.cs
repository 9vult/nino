using System.Text;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Services
{
    internal class CongaReminderService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private const int OneHour = 60 * 60 * 1000;
        private readonly System.Timers.Timer _timer;

        public CongaReminderService()
        {
            _timer = new System.Timers.Timer
            {
                Interval = OneHour
            };
            _timer.Elapsed += async (_, _) => await RemindTardyTasks();
            _timer.Start();
        }

        private static async System.Threading.Tasks.Task RemindTardyTasks()
        {
            foreach (var project in Cache.GetProjects().Where(p => p.CongaReminderEnabled))
            {
                if (await Nino.Client.GetChannelAsync((ulong)project.CongaReminderChannelId!) is not SocketTextChannel channel) continue;
                var gLng = Cache.GetConfig(project.GuildId)?.Locale?.ToDiscordLocale() ?? channel.Guild.PreferredLocale;
                
                var reminderText = new StringBuilder();
                foreach (var episode in Cache.GetEpisodes(project.Id).Where(e => !e.Tasks.All(t => t.Done)))
                {
                    var patchOperations = new List<PatchOperation>();
                    foreach (var abbreviation in GetTardyTasks(project, episode))
                    {
                        var keyStaff = project.KeyStaff.FirstOrDefault(ks => ks.Role.Abbreviation == abbreviation);
                        if (keyStaff is null) continue;

                        var staffMention = $"<@{keyStaff.UserId}>";
                        var roleTitle = keyStaff.Role.Name;
                        reminderText.AppendLine(T("progress.done.conga.reminder", gLng, staffMention, episode.Number, roleTitle));
                        
                        // Update database with new last-reminded time
                        var taskIndex = Array.IndexOf(episode.Tasks, episode.Tasks.Single(t => t.Abbreviation == abbreviation));
                        patchOperations.Add(PatchOperation.Set($"/tasks/{taskIndex}/lastReminded", DateTimeOffset.Now));
                    }
                    
                    if (patchOperations.Count != 0)
                        await AzureHelper.PatchEpisodeAsync(episode, patchOperations);
                }

                if (reminderText.Length <= 0) continue;
                await channel.SendMessageAsync(reminderText.ToString());
                Log.Info($"Published conga reminders for {project}");
                
                await Cache.RebuildCacheForProject(project.Id);
            }
        }

        /// <summary>
        /// Get a list of all currently-tardy tasks for an episode
        /// </summary>
        /// <param name="project">Project the episode is from</param>
        /// <param name="episode">The episode to check</param>
        /// <returns>A list of tardy task abbreviations</returns>
        private static List<string> GetTardyTasks(Project project, Episode episode)
        {
            var taskLookup = episode.Tasks.ToDictionary(t => t.Abbreviation, t => t);
            
            // Group by next to find prerequisites
            var prerequisiteGroups = project.CongaParticipants
                .GroupBy(p => p.Next)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => p.Current).ToList()
                );
            
            var nextTasks = new List<string>();

            foreach (var (nextTask, prerequisites) in prerequisiteGroups)
            {
                // Check if all prereqs are complete
                if (!prerequisites.All(p => taskLookup.TryGetValue(p, out var pTask) && pTask.Done)) continue;
                if (!taskLookup.TryGetValue(nextTask, out var task) || task.Done) continue;
                
                var mostRecentlyUpdated = prerequisites
                    .Select(p => taskLookup.TryGetValue(p, out var pTask) && pTask.Done ? pTask.Updated : null)
                    .Max();
                        
                // Add to the list if the task is indeed tardy
                if (!taskLookup.TryGetValue(nextTask, out var candidate) || candidate.LastReminded is null) continue;
                if (candidate.LastReminded < DateTimeOffset.Now - project.CongaReminderPeriod)
                    nextTasks.Add(nextTask);
            }
            
            return nextTasks;
        }
    }
}
