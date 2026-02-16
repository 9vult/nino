// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Dtos;

namespace Nino.Core.Services;

public class NotificationDataService(DataContext db, ILogger<NotificationDataService> logger)
    : INotificationDataService
{
    /// <inheritdoc />
    public async Task<AirNotificationDto?> GetAirNotificationDataAsync(
        Guid projectId,
        Guid episodeId
    )
    {
        var project = await db
            .Projects.Include(p => p.AirReminderChannel)
            .Include(p => p.AirReminderUser)
            .Include(p => p.AirReminderRole)
            .SingleOrDefaultAsync(p => p.Id == projectId);
        var episode = await db.Episodes.SingleOrDefaultAsync(e => e.Id == episodeId);

        if (project is null)
        {
            logger.LogWarning("Failed to find a project with id {ProjectId}", projectId);
            return null;
        }

        if (episode is null)
        {
            logger.LogWarning("Failed to find a episode with id {EpisodeId}", episodeId);
            return null;
        }

        var locale = (await db.Groups.SingleOrDefaultAsync(g => g.Id == project.GroupId))
            ?.Configuration
            .Locale;

        return new AirNotificationDto(
            ProjectTitle: project.Title,
            ProjectType: project.Type,
            AniListUrl: project.AniListUrl,
            PosterUrl: project.PosterUri,
            EpisodeNumber: episode.Number,
            NotificationChannel: project.AirReminderChannel,
            NotificationUser: project.AirReminderUser,
            NotificationRole: project.AirReminderRole,
            NotificationLocale: locale
        );
    }
}
