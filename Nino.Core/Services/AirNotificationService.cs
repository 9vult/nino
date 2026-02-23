// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Events;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Services;

public class AirNotificationService(
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
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            var aniListService = scope.ServiceProvider.GetRequiredService<AniListService>();

            var targets = await db
                .Projects.Include(p => p.Episodes)
                .Where(p => !p.IsArchived && p.AirNotificationsEnabled && p.AniListId > 0)
                .ToListAsync();

            foreach (var project in targets)
            {
                var episodeNumber = 0m;
                foreach (
                    var episode in project.Episodes.Where(e =>
                        e is { IsDone: false, AirNotificationPosted: false }
                        && Episode.EpisodeNumberIsNumber(e.Number, out episodeNumber)
                    )
                )
                {
                    var (status, airTime) = await aniListService.GetEpisodeAirTimeAsync(
                        project.AniListId,
                        episodeNumber
                    );
                    if (status != ResultStatus.Success)
                        continue;
                    airTime = airTime.Add(project.AirNotificationDelay);
                    if (airTime > DateTimeOffset.UtcNow)
                        continue;

                    logger.LogTrace("Publishing Episode Aired event for {Episode}", episode);

                    var airEvent = new EpisodeAiredEvent(project.Id, episode.Id, airTime);
                    await eventBus.PublishAsync(airEvent);
                    episode.AirNotificationPosted = true;

                    // Conga time!

                    var airNode = project.CongaParticipants.Get("$AIR");
                    if (airNode is null)
                        continue;

                    var combinedStaff = project
                        .GetCombinedStaff(episode)
                        .Where(s => s.UserId != project.AirNotificationUserId) // Prevent double pings
                        .ToList();
                    var activatedNodes = airNode.GetActivatedNodes(episode);
                    foreach (var node in activatedNodes)
                    {
                        var staff = combinedStaff.SingleOrDefault(t =>
                            t.Role.Abbreviation == node.Abbreviation
                        );
                        if (staff is null)
                            continue;

                        var task = episode.Tasks.Single(t => t.Abbreviation == node.Abbreviation);
                        logger.LogTrace("Publishing Conga event for {Task}", task);

                        var congaEvent = new CongaNotificationEvent(
                            project.Id,
                            episode.Id,
                            task.Abbreviation,
                            IsReminder: false
                        );
                        await eventBus.PublishAsync(congaEvent);

                        task.LastRemindedAt = DateTimeOffset.UtcNow;
                    }
                }
            }
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Air Notification Service error: {Message}", ex.Message);
        }
    }
}
