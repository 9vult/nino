// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Domain.ValueObjects;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Services;

public sealed class AirNotificationService(
    IServiceScopeFactory scopeFactory,
    IEventBus eventBus,
    ILogger<AirNotificationService> logger
) : BackgroundService
{
    private static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Air Notification Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(FiveMinutes, stoppingToken);
            await CheckForNewReleasesAsync();
        }
    }

    private async Task CheckForNewReleasesAsync()
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<NinoDbContext>();
            var aniListService = scope.ServiceProvider.GetRequiredService<IAniListService>();

            var episodes = await db
                .Episodes.Include(e => e.Project)
                .Where(e =>
                    !e.Project.IsArchived
                    && e.Project.AirNotificationsEnabled
                    && e.Project.AniListId.Value > 0
                    && !e.IsDone
                    && !e.AirNotificationPosted
                )
                .ToListAsync();

            foreach (var episode in episodes)
            {
                if (!episode.Number.IsDecimal(out var episodeNumber))
                    continue;

                var aniListResult = await aniListService.GetEpisodeAirTimeAsync(
                    episode.Project.AniListId,
                    episodeNumber
                );
                if (!aniListResult.IsSuccess)
                    continue;

                var airTime = aniListResult.Value.Add(episode.Project.AirNotificationDelay);
                if (airTime > DateTimeOffset.UtcNow)
                    continue;

                logger.LogTrace("Publishing Episode Aired event for {Episode}", episode);

                var airEvent = new EpisodeAiredEvent(episode.ProjectId, episode.Id, airTime);
                await eventBus.PublishAsync(airEvent);
                episode.AirNotificationPosted = true;

                // Conga time!

                if (
                    !episode.Project.CongaParticipants.TryGetNode(
                        Abbreviation.From("$AIR"),
                        out var airNode
                    )
                )
                    continue;

                var activatedNodes = airNode.GetActivatedNodes(episode.Tasks.ToList());
                var tasks = activatedNodes
                    .Select(n => episode.Tasks.FirstOrDefault(t => t.Abbreviation == n.Name))
                    .OfType<Domain.Entities.Task>() // Eliminate nulls
                    .ToList();

                if (tasks.Count == 0)
                    continue;

                foreach (var task in tasks)
                    task.LastRemindedAt = DateTimeOffset.UtcNow;

                var congaEvent = new CongaNotificationEvent(
                    episode.ProjectId,
                    episode.Id,
                    tasks.Select(t => t.Id).ToList(),
                    IsReminder: false
                );
                await eventBus.PublishAsync(congaEvent);
            }
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Air notification service error: {Message}", ex.Message);
        }
    }
}
