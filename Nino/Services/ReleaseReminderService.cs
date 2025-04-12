using Discord;
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
                _timer.Stop();
                await CheckForReleases();
            };
            _timer.Start();
        }

        private static async System.Threading.Tasks.Task CheckForReleases()
        {
            Dictionary<Guid, List<Episode>> marked = [];
            foreach (var project in Cache.GetProjects().Where(p => p.AirReminderEnabled && p.AniListId is not null))
            {
                var decimalNumber = 0m;
                foreach (var episode in Cache.GetEpisodes(project.Id).Where(e => !e.Done && !e.ReminderPosted && Utils.EpisodeNumberIsNumber(e.Number, out decimalNumber)))
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
    }
}
