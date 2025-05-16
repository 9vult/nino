using System.Text;
using Discord;
using Discord.WebSocket;
using Localizer;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;
using Task = System.Threading.Tasks.Task;

namespace Nino.Services
{
    internal class ReleaseReminderService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private const int FiveMinutes = 5 * 60 * 1000;
        private readonly System.Timers.Timer _timer;

        public ReleaseReminderService()
        {
            _timer = new System.Timers.Timer
            {
                Interval = FiveMinutes
            };
            _timer.Elapsed += async (_, _) =>
            {
                await CheckForReleases();
            };
            _timer.Start();
        }

        private static async System.Threading.Tasks.Task CheckForReleases()
        {
            Dictionary<Guid, List<Episode>> marked = [];
            
            foreach (var project in Cache.GetProjects().Where(p => p is { AirReminderEnabled: true, AniListId: not null }))
            {
                var decimalNumber = 0m;
                foreach (var episode in Cache.GetEpisodes(project.Id).Where(e => e is { Done: false, ReminderPosted: false } && Utils.EpisodeNumberIsNumber(e.Number, out decimalNumber)))
                {
                    try
                    {
                        var airTime = await AirDateService.GetAirDate((int)project.AniListId!, decimalNumber + (project.AniListOffset ?? 0));
                        if (airTime is null || DateTimeOffset.UtcNow < airTime)
                            continue;

                        if (await Nino.Client.GetChannelAsync((ulong)project.AirReminderChannelId!) is not SocketTextChannel channel) continue;
                        var gLng = Cache.GetConfig(channel.Guild.Id)?.Locale?.ToDiscordLocale() ?? channel.Guild.PreferredLocale;
                        
                        if (!marked.TryGetValue(project.Id, out var markedList))
                        {
                            markedList = [];
                            marked[project.Id] = markedList;
                        }
                        markedList.Add(episode);

                        var role = project.AirReminderRoleId is not null
                            ? project.AirReminderRoleId == project.GuildId ? "@everyone" : $"<@&{project.AirReminderRoleId}>"
                            : string.Empty;
                        var member = project.AirReminderUserId is not null
                            ? $"<@{project.AirReminderUserId}>"
                            : string.Empty;
                        var mention = $"{role}{member}";
                        var embed = new EmbedBuilder()
                            .WithAuthor($"{project.Title} ({project.Type.ToFriendlyString(gLng)})", url: project.AniListUrl)
                            .WithTitle(T("title.aired", gLng, episode.Number))
                            .WithDescription(await AirDateService.GetAirDateString((int)project.AniListId!, decimalNumber + (project.AniListOffset ?? 0), gLng))
                            .WithThumbnailUrl(project.PosterUri)
                            .WithCurrentTimestamp()
                            .Build();
                        await channel.SendMessageAsync(text: mention, embed: embed);
                        
                        // Conga intervention!
                        await DoReleaseConga(project, episode, gLng);
                        
                        Log.Info($"Published release reminder for {project} episode {episode}");
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                        // TODO: Error alerting
                    }
                }
            }

            // Update database
            foreach (var kvpair in marked)
            {
                TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(kvpair.Key.ToString()));
                foreach (var episode in kvpair.Value)
                {
                    batch.PatchItem(id: episode.Id.ToString(), [
                        PatchOperation.Set("/reminderPosted", true)
                    ]);
                }
                await batch.ExecuteAsync();
                await Cache.RebuildCacheForProject(kvpair.Key);
            }
        }

        /// <summary>
        /// Helper function for $AIR conga reminders
        /// </summary>
        /// <param name="project">The project</param>
        /// <param name="episode">The episode</param>
        /// <param name="gLng">Guild language</param>
        private static async Task DoReleaseConga(Project project, Episode episode, string gLng)
        {
            var prefixMode = Cache.GetConfig(project.GuildId)?.CongaPrefix ?? CongaPrefixType.None;
            var reminderText = new StringBuilder();
            if (project.CongaParticipants.Nodes.Count != 0)
            {
                var staff = project.KeyStaff.Concat(episode.AdditionalStaff).ToList();
                var validTargets = new HashSet<string>(staff
                    .Select(ks => ks.Role.Abbreviation)
                    .Where(ks => episode.Tasks.FirstOrDefault(t => t.Abbreviation == ks) is { Done: false, LastReminded: null})
                );

                var nodes = project.CongaParticipants
                    .GetDependentsOf("$AIR")
                    .Where(c => validTargets.Contains(c.Abbreviation))
                    .ToList();

                var pingTargets = nodes
                    .Select(c => staff.FirstOrDefault(ks => ks.Role.Abbreviation == c.Abbreviation))
                    .Where(c => c?.UserId != project.AirReminderUserId) // Prevent double pings
                    .ToList();
                        
                var patchOperations = nodes
                    .Select(c => Array.FindIndex(episode.Tasks, t => t.Abbreviation == c.Abbreviation))
                    .Where(index => index != -1)
                    .Select(index => PatchOperation.Set($"/tasks/{index}/lastReminded", DateTimeOffset.UtcNow))
                    .ToList();

                if (patchOperations.Count == 0) return;
                await AzureHelper.PatchEpisodeAsync(episode, patchOperations);
                
                // Time to send the conga message
                if (await Nino.Client.GetChannelAsync((ulong)project.CongaReminderChannelId!) is not SocketTextChannel channel) return;

                foreach (var target in pingTargets)
                {
                    if (target is null) continue;

                    var staffMention = $"<@{target.UserId}>";
                    var roleTitle = target.Role.Name;
                    if (prefixMode != CongaPrefixType.None)
                    {
                        reminderText.Append($"[{prefixMode switch {
                            CongaPrefixType.Nickname => project.Nickname,
                            CongaPrefixType.Title => project.Title,
                            _ => string.Empty
                        }}] ");
                    }
                    reminderText.AppendLine(T("progress.done.conga", gLng, staffMention, episode.Number, roleTitle));
                }
                
                if (reminderText.Length <= 0) return;
                await channel.SendMessageAsync(reminderText.ToString());
                Log.Info($"Published release conga reminders for {project} episode {episode}");
            }
        }
    }
}
