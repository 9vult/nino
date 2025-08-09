using System.Text;
using Discord;
using Discord.WebSocket;
using Localizer;
using Microsoft.EntityFrameworkCore;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities.Extensions;
using NLog;
using static Localizer.Localizer;
using Configuration = Nino.Records.Configuration;
using Task = System.Threading.Tasks.Task;

namespace Nino.Services
{
    internal class ReleaseReminderService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private const int FiveMinutes = 5 * 60 * 1000;
        private readonly System.Timers.Timer _timer;

        public ReleaseReminderService(DataContext db)
        {
            _timer = new System.Timers.Timer
            {
                Interval = FiveMinutes
            };
            _timer.Elapsed += async (_, _) =>
            {
                await CheckForReleases(db);
            };
            _timer.Start();
        }

        private static async Task CheckForReleases(DataContext db)
        {
            Dictionary<Guid, List<Episode>> marked = [];
            
            foreach (var project in db.Projects.Include(p => p.Episodes).Where(p => p.AirReminderEnabled && p.AniListId != null ))
            {
                var decimalNumber = 0m;
                foreach (var episode in project.Episodes.Where(e => e is { Done: false, ReminderPosted: false } && Episode.EpisodeNumberIsNumber(e.Number, out decimalNumber)))
                {
                    try
                    {
                        var airTime = await AirDateService.GetAirDate((int)project.AniListId!, decimalNumber + (project.AniListOffset ?? 0));
                        if (airTime is null || DateTimeOffset.UtcNow < airTime)
                            continue;

                        if (await Nino.Client.GetChannelAsync((ulong)project.AirReminderChannelId!) is not SocketTextChannel channel) continue;
                        var config = db.GetConfig(channel.Guild.Id);
                        var gLng = config?.Locale?.ToDiscordLocale() ?? channel.Guild.PreferredLocale;
                        
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
                        await DoReleaseConga(project, episode, gLng, config);
                        
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
            foreach (var episode in marked.SelectMany(kvp => kvp.Value))
            {
                episode.ReminderPosted = true;
            }
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Helper function for $AIR conga reminders
        /// </summary>
        /// <param name="project">The project</param>
        /// <param name="episode">The episode</param>
        /// <param name="gLng">Guild language</param>
        /// <param name="config">Configuration</param>
        private static async Task DoReleaseConga(Project project, Episode episode, string gLng, Configuration? config)
        {
            var prefixMode = config?.CongaPrefix ?? CongaPrefixType.None;
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
                    .Where(c => c is not null)
                    .Where(c => c?.UserId != project.AirReminderUserId) // Prevent double pings
                    .ToList();

                foreach (var task in nodes
                             .Select(n => episode.Tasks.FirstOrDefault(t => t.Abbreviation == n.Abbreviation))
                             .Where(t => t is not null))
                {
                    task!.LastReminded = DateTimeOffset.UtcNow;
                }
                
                // Time to send the conga message
                if (await Nino.Client.GetChannelAsync((ulong)project.AirReminderChannelId!) is not SocketTextChannel channel) return;

                foreach (var target in pingTargets)
                {
                    if (target is null) continue;

                    var userId = episode.PinchHitters.FirstOrDefault(t => t.Abbreviation == target.Role.Abbreviation)?.UserId ?? target.UserId;
                    var staffMention = $"<@{userId}>";
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
