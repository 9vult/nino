// SPDX-License-Identifier: MPL-2.0

using NaturalSort.Extension;
using Nino.Core.Dtos;
using Nino.Core.Entities;
using Nino.Core.Enums;

namespace Nino.Core.Services;

public class DataService(DataContext db, IAniListService aniListService) : IDataService
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
    public async Task<TaskProgressDto> GetTaskProgressDataAsync(
        Guid projectId,
        string episodeNumber,
        string abbreviation
    )
    {
        var project = await db.Projects.SingleAsync(p => p.Id == projectId);
        var episode = await db.Episodes.SingleAsync(e =>
            e.ProjectId == projectId && e.Number == episodeNumber
        );
        var config = await db
            .Groups.Where(g => g.Id == project.GroupId)
            .Select(g => g.Configuration)
            .SingleAsync();

        return new TaskProgressDto
        {
            Abbreviation = abbreviation,
            FullName = project
                .KeyStaff.Concat(episode.AdditionalStaff)
                .Single(s => s.Role.Abbreviation == abbreviation)
                .Role.Abbreviation,
            ProgressResponseType = config.ProgressResponseType,
            ProgressPublishType = config.ProgressPublishType,
        };
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

    /// <inheritdoc />
    public async Task<string?> GetWorkingEpisodeAsync(Guid projectId)
    {
        var candidates = await db
            .Episodes.Where(e => e.ProjectId == projectId && !e.IsDone)
            .Select(e => e.Number)
            .ToListAsync();

        if (candidates.Count == 0)
            return null;

        return candidates
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase.WithNaturalSort())
            .First();
    }

    /// <inheritdoc />
    public async Task<string?> GetWorkingTaskEpisodeAsync(Guid projectId, string abbreviation)
    {
        var candidates = await db
            .Episodes.Where(e => e.ProjectId == projectId && !e.IsDone)
            .Select(e => new { e.Number, e.Tasks })
            .ToListAsync();

        if (candidates.Count == 0)
            return null;

        return candidates
            .OrderBy(e => e.Number, StringComparer.OrdinalIgnoreCase.WithNaturalSort())
            .FirstOrDefault(e =>
                e.Tasks.SingleOrDefault(t => t.Abbreviation == abbreviation)?.IsDone is false
            )
            ?.Number;
    }

    /// <inheritdoc />
    public async Task<int> GetEpisodeDifferenceAsync(
        Guid projectId,
        string firstEpisodeNumber,
        string secondEpisodeNumber
    )
    {
        var episodes = (await db.Episodes.Where(p => p.Id == projectId).ToListAsync())
            .OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        var firstIdx = episodes.FindIndex(e => e.Number == firstEpisodeNumber);
        var lastIdx = episodes.FindIndex(e => e.Number == secondEpisodeNumber);
        return lastIdx - firstIdx;
    }

    /// <inheritdoc />
    public async Task<bool> GetHasEpisodeAiredAsync(Guid projectId, string episodeNumber)
    {
        var aniListId = await db
            .Projects.Where(p => p.Id == projectId)
            .Select(p => p.AniListId)
            .SingleAsync();

        if (aniListId <= 0)
            return true;

        if (decimal.TryParse(episodeNumber, out var dec))
        {
            var result = await aniListService.EpisodeHasAiredAsync(aniListId, dec);
            if (result.Status == ResultStatus.Success)
                return result.Value;
        }
        return true;
    }

    /// <inheritdoc />
    public async Task<string> GetTaskNameAsync(
        Guid projectId,
        string episodeNumber,
        string abbreviation
    )
    {
        var keyStaff = await db
            .Projects.Where(p => p.Id == projectId)
            .Select(p => p.KeyStaff)
            .SingleAsync();
        var name = keyStaff.SingleOrDefault(s => s.Role.Abbreviation == abbreviation)?.Role.Name;
        if (name is not null)
            return name;

        var additionalStaff = await db
            .Episodes.Where(e => e.ProjectId == projectId && e.Number == episodeNumber)
            .Select(e => e.AdditionalStaff)
            .SingleAsync();

        return additionalStaff.Single(s => s.Role.Abbreviation == abbreviation).Role.Name;
    }

    /// <inheritdoc />
    public async Task<bool> GetDoesTaskExistAsync(Guid projectId, string abbreviation)
    {
        return await db
            .Episodes.Where(e => e.ProjectId == projectId)
            .AnyAsync(e => e.Tasks.Any(t => t.Abbreviation == abbreviation));
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
