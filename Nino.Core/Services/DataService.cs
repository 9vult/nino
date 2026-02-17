// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Dtos;

namespace Nino.Core.Services;

public class DataService(DataContext db, ILogger<DataService> logger) : IDataService
{
    /// <inheritdoc />
    public async Task<AirNotificationDto> GetAirNotificationDataAsync(
        Guid projectId,
        Guid episodeId
    )
    {
        var project = await db
            .Projects.Include(p => p.AirReminderChannel)
            .Include(p => p.AirReminderUser)
            .Include(p => p.AirReminderRole)
            .SingleAsync(p => p.Id == projectId);
        var episode = await db.Episodes.SingleAsync(e => e.Id == episodeId);

        var locale = (await db.Groups.SingleAsync(g => g.Id == project.GroupId))
            .Configuration
            .Locale;

        return new AirNotificationDto(
            ProjectTitle: project.Title,
            ProjectType: project.Type,
            AniListUrl: project.AniListUrl,
            PosterUrl: project.PosterUri,
            EpisodeNumber: episode.Number,
            NotificationChannel: project.AirReminderChannel?.AsMappedId(),
            NotificationUser: project.AirReminderUser,
            NotificationRole: project.AirReminderRole,
            NotificationLocale: locale
        );
    }

    /// <inheritdoc />
    public async Task<ProjectBasicInfoDto> GetProjectBasicInfoAsync(Guid projectId)
    {
        var project = await db.Projects.SingleAsync(p => p.Id == projectId);

        return new ProjectBasicInfoDto(
            Nickname: project.Nickname,
            Title: project.Title,
            Type: project.Type,
            IsPrivate: project.IsPrivate
        );
    }
}
