using System.Text;
using Discord.WebSocket;
using Localizer;
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
            foreach (var project in Cache.GetProjects().Where(p => p is { CongaReminderEnabled: true, IsArchived: false }))
            {
                if (await Nino.Client.GetChannelAsync((ulong)project.CongaReminderChannelId!) is not SocketTextChannel channel) continue;
                var gLng = Cache.GetConfig(project.GuildId)?.Locale?.ToDiscordLocale() ?? channel.Guild.PreferredLocale;
                
                var prefixMode = Cache.GetConfig(project.GuildId)?.CongaPrefix ?? CongaPrefixType.None;
                var reminderText = new StringBuilder();
                foreach (var episode in Cache.GetEpisodes(project.Id).Where(e => !e.Tasks.All(t => t.Done)))
                {
                    var patchOperations = new List<PatchOperation>();
                    foreach (var abbreviation in Utils.GetTardyTasks(project, episode))
                    {
                        var keyStaff = project.KeyStaff.FirstOrDefault(ks => ks.Role.Abbreviation == abbreviation);
                        if (keyStaff is null) continue;

                        var staffMention = $"<@{keyStaff.UserId}>";
                        var roleTitle = keyStaff.Role.Name;
                        if (prefixMode != CongaPrefixType.None)
                        {
                            // Using a switch expression in the middle of string interpolation is insane btw
                            reminderText.Append($"[{prefixMode switch {
                                CongaPrefixType.Nickname => project.Nickname,
                                CongaPrefixType.Title => project.Title,
                                _ => string.Empty 
                            }}] ");
                        }
                        reminderText.AppendLine(T("progress.done.conga.reminder", gLng, staffMention, episode.Number, roleTitle));
                        
                        // Update database with new last-reminded time
                        var taskIndex = Array.IndexOf(episode.Tasks, episode.Tasks.Single(t => t.Abbreviation == abbreviation));
                        patchOperations.Add(PatchOperation.Set($"/tasks/{taskIndex}/lastReminded", DateTimeOffset.UtcNow));
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
    }
}
