// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using Nino.Core.Entities;
using Nino.Core.Enums;

namespace Nino.Core.Actions.Project.Create;

public class ProjectCreateHandler(DataContext db, ILogger<ProjectCreateHandler> logger)
{
    public async Task<ProjectCreateResult> HandleAsync(ProjectCreateAction action)
    {
        var dto = action.Dto;

        // Sanitize nickname
        dto.Nickname = dto.Nickname.Trim().ToLowerInvariant().Replace(" ", string.Empty);

        if (
            await db.Projects.AnyAsync(p =>
                p.GroupId == action.GroupId && p.Nickname == dto.Nickname
            )
        )
            return new ProjectCreateResult(ActionStatus.Conflict, null, dto.Nickname);

        // TODO: Temp
        dto.Title ??= "Kono Subarashii Sekai ni Shukufuku wo!";
        dto.Type ??= ProjectType.TV;
        dto.Length ??= 10;
        dto.PosterUri ??=
            "https://s4.anilist.co/file/anilistcdn/media/anime/cover/large/bx21202-qQoJeKz76vRT.png";

        var project = new Entities.Project
        {
            Id = Guid.NewGuid(),
            GroupId = action.GroupId,
            Nickname = dto.Nickname,
            Title = dto.Title,
            OwnerId = action.OwnerId,
            Type = dto.Type.Value,
            PosterUri = dto.PosterUri,
            UpdateChannelId = dto.UpdateChannelId,
            ReleaseChannelId = dto.ReleaseChannelId,
            IsPrivate = dto.IsPrivate,
            IsArchived = false,
            CongaParticipants = new CongaGraph(),
            Motd = string.Empty,
            AniListId = dto.AniListId,
            AniListOffset = 0,
            AirReminderEnabled = false,
            CongaReminderEnabled = false,
            Created = DateTimeOffset.UtcNow,
        };
        logger.LogInformation("Creating project {Project}", project);
        await db.Projects.AddAsync(project);

        var episodes = new List<Episode>();
        for (var i = dto.FirstEpisode; i < dto.FirstEpisode + dto.Length; i++)
        {
            episodes.Add(
                new Episode
                {
                    GroupId = action.GroupId,
                    ProjectId = project.Id,
                    Number = Convert.ToString(i, CultureInfo.InvariantCulture),
                    Done = false,
                    ReminderPosted = false,
                    AdditionalStaff = [],
                    PinchHitters = [],
                    Tasks = [],
                }
            );
        }

        logger.LogInformation(
            "Creating {EpisodeCount} episodes for {Project}",
            episodes.Count,
            project
        );
        await db.Episodes.AddRangeAsync(episodes);

        await db.SaveChangesAsync();

        return new ProjectCreateResult(ActionStatus.Success, project.Id, project.Nickname);
    }
}
