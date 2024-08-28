using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Commands;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using System;

namespace Nino.Services
{
    internal class ReleaseReminderService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        private const int FIVE_MINUTES = 1 * 60 * 1000;
        private readonly System.Timers.Timer _timer;

        public ReleaseReminderService()
        {
            _timer = new System.Timers.Timer
            {
                Interval = FIVE_MINUTES
            };
            _timer.Elapsed += async (object? s, System.Timers.ElapsedEventArgs e) => await CheckForReleases();
            _timer.Start();
        }

        private async System.Threading.Tasks.Task CheckForReleases()
        {
            Dictionary<string, List<CachedEpisode>> marked = [];
            foreach (var project in Cache.GetProjects().Where(p => p.AirReminderEnabled && !string.IsNullOrEmpty(p.AniDBId)))
            {
                foreach (var episode in Cache.GetEpisodes(project.Id).Where(e => !e.Done && !e.ReminderPosted))
                {
                    var airTime = await AirDateService.GetAirDate(project.AniDBId!, episode.Number, project.AirTime ?? "00:00");
                    if (DateTimeOffset.Now < airTime)
                        continue;

                    try
                    {
                        if (await Nino.Client.GetChannelAsync((ulong)project.AirReminderChannelId!) is not SocketTextChannel channel) continue;
                        
                        if (!marked.TryGetValue(project.Id, out var markedList))
                        {
                            markedList = [];
                            marked[project.Id] = markedList;
                        }
                        markedList.Add(episode);

                        var role = project.AirReminderRoleId != null
                            ? project.AirReminderRoleId == project.GuildId ? "@everyone" : $"<@&{project.AirReminderRoleId}>"
                            : "";
                        var embed = new EmbedBuilder()
                            .WithAuthor($"{project.Title} ({project.Type.ToFriendlyString()})")
                            .WithTitle($"Episode {episode.Number} has aired!")
                            .WithDescription(await AirDateService.GetAirDateString(project.AniDBId!, episode.Number, project.AirTime ?? "00:00"))
                            .WithThumbnailUrl(project.PosterUri)
                            .WithCurrentTimestamp()
                            .Build();
                        await channel.SendMessageAsync(text: role, embed: embed);
                    }
                    catch (Exception e)
                    {
                        log.Error(e.Message);
                        // TODO: Error alerting
                    }
                }
            }

            // Update database
            foreach (var kvpair in marked)
            {
                TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(kvpair.Key));
                foreach (var episode in kvpair.Value)
                {
                    batch.PatchItem(id: episode.Id, new[]
                    {
                        PatchOperation.Set("/reminderPosted", true)
                    });
                }
                await batch.ExecuteAsync();
                await Cache.RebuildCacheForProject(kvpair.Key);
            }
        }
    }
}
