// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Dtos;
using Nino.Core.Entities;

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

    /// <inheritdoc />
    public async Task<EpisodeStatusDto> GetEpisodeStatusAsync(Guid projectId, string episodeNumber)
    {
        var project = await db.Projects.SingleAsync(p => p.Id == projectId);
        var episode = await db.Episodes.SingleAsync(e =>
            e.ProjectId == projectId && e.Number == episodeNumber
        );
        return MapEpisodeStatus(project, episode);
    }

    /// <inheritdoc />
    public async Task<List<EpisodeStatusDto>> GetEpisodeStatusAsync(
        Guid projectId,
        IList<string> episodeNumbers
    )
    {
        var project = await db.Projects.SingleAsync(p => p.Id == projectId);
        var episodes = await db
            .Episodes.Where(e => e.ProjectId == projectId && episodeNumbers.Contains(e.Number))
            .ToListAsync();

        return episodes.Select(e => MapEpisodeStatus(project, e)).ToList();
    }

    private static EpisodeStatusDto MapEpisodeStatus(Project project, Episode episode)
    {
        var staff = project
            .KeyStaff.Concat(episode.AdditionalStaff)
            .ToDictionary(s => s.Role.Abbreviation);

        return new EpisodeStatusDto
        {
            Number = episode.Number,
            IsDone = episode.IsDone,
            UpdatedAt = episode.UpdatedAt,
            Tasks = episode
                .Tasks.Select(t =>
                {
                    var s = staff[t.Abbreviation];
                    return new TaskStatusDto
                    {
                        Abbreviation = t.Abbreviation,
                        Weight = s.Role.Weight,
                        User = MappedIdDto.FromMappedId(s.User),
                        IsPseudo = false,
                        IsDone = t.IsDone,
                    };
                })
                .ToArray(),
        };
    }
}
