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
            .Projects.Include(p => p.AirNotificationChannel)
            .Include(p => p.AirNotificationUser)
            .Include(p => p.AirNotificationRole)
            .Where(p => p.Id == projectId)
            .Select(p => new
            {
                p.GroupId,
                p.Title,
                p.Type,
                p.AniListUrl,
                p.PosterUrl,
                p.AirNotificationChannel,
                p.AirNotificationUser,
                p.AirNotificationRole,
            })
            .SingleAsync();

        var episode = await db
            .Episodes.Where(e => e.Id == episodeId)
            .Select(e => new { e.Number })
            .SingleAsync();

        var locale = await db
            .Groups.Where(g => g.Id == project.GroupId)
            .Select(g => g.Configuration.Locale)
            .SingleAsync();

        return new AirNotificationDto(
            ProjectTitle: project.Title,
            ProjectType: project.Type,
            AniListUrl: project.AniListUrl,
            PosterUrl: project.PosterUrl,
            EpisodeNumber: episode.Number,
            NotificationChannel: project.AirNotificationChannel?.AsMappedId(),
            NotificationUser: project.AirNotificationUser,
            NotificationRole: project.AirNotificationRole,
            NotificationLocale: locale
        );
    }

    /// <inheritdoc />
    public async Task<ProjectBasicInfoDto> GetProjectBasicInfoAsync(Guid projectId)
    {
        return await db
            .Projects.Where(p => p.Id == projectId)
            .Select(p => new ProjectBasicInfoDto(
                Nickname: p.Nickname,
                Title: p.Title,
                Type: p.Type,
                IsPrivate: p.IsPrivate
            ))
            .SingleAsync();
    }

    /// <inheritdoc />
    public async Task<ProjectCompletionStatusDto> GetProjectCompletionStatusAsync(Guid projectId)
    {
        var counts = await db
            .Episodes.Where(e => e.ProjectId == projectId)
            .GroupBy(_ => true)
            .Select(g => new
            {
                Completed = g.Count(e => e.IsDone),
                Incomplete = g.Count(e => !e.IsDone),
            })
            .FirstOrDefaultAsync();

        return new ProjectCompletionStatusDto(
            CompletedEpisodeCount: counts?.Completed ?? 0,
            IncompleteEpisodeCount: counts?.Incomplete ?? 0
        );
    }
}
