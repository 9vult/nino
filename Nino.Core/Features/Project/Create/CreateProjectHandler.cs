// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using Nino.Core.Dtos;
using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.Project.Create;

public sealed class CreateProjectHandler(
    DataContext db,
    IAniListService aniListService,
    IUserVerificationService verificationService,
    ILogger<CreateProjectHandler> logger
)
{
    private const string FallbackPosterUrl = "https://files.catbox.moe/j3qizm.png";

    public async Task<Result<CreateProjectResponse>> HandleAsync(CreateProjectCommand input)
    {
        var (dto, groupId, ownerId, overrideVerification) = input;
        if (
            !overrideVerification
            && !await verificationService.VerifyGroupPermissionsAsync(
                groupId,
                ownerId,
                PermissionsLevel.Administrator
            )
        )
            return new Result<CreateProjectResponse>(ResultStatus.Unauthorized);

        // Sanitize nickname
        dto.Nickname = dto.Nickname.Trim().ToLowerInvariant().Replace(" ", string.Empty);

        if (
            await db.Projects.AnyAsync(p =>
                p.GroupId == input.GroupId && p.Nickname == dto.Nickname
            )
        )
            return new Result<CreateProjectResponse>(ResultStatus.Conflict, null);

        var autoFields = GetAutoFields(dto);
        if (!string.IsNullOrEmpty(autoFields))
            logger.LogInformation(
                "AniList will be used in the construction of project {Nickname} for the following fields: {Fields}",
                dto.Nickname,
                autoFields
            );

        var anime = await aniListService.GetAnimeAsync(dto.AniListId);
        if (anime.Status is ResultStatus.Success)
        {
            dto.Title ??= anime.Title;
            dto.Length ??= anime.EpisodeCount;
            dto.Type ??= anime.Type;
            dto.PosterUri ??= anime.PosterUrl ?? FallbackPosterUrl;
        }
        else if (anime.Status is not ResultStatus.BadRequest) // Any other error (bad request is AniListId <= 0)
            return new Result<CreateProjectResponse>(ResultStatus.Error);

        if (dto.Title is null || dto.Length is null || dto.Type is null || dto.PosterUri is null)
            return new Result<CreateProjectResponse>(ResultStatus.BadRequest);

        var project = new Entities.Project
        {
            Id = Guid.NewGuid(),
            GroupId = input.GroupId,
            Nickname = dto.Nickname,
            Title = dto.Title,
            OwnerId = input.OwnerId,
            Type = dto.Type.Value,
            PosterUrl = dto.PosterUri,
            ProjectChannelId = dto.ProjectChannelId,
            UpdateChannelId = dto.UpdateChannelId,
            ReleaseChannelId = dto.ReleaseChannelId,
            IsPrivate = dto.IsPrivate,
            IsArchived = false,
            CongaParticipants = new CongaGraph(),
            Motd = string.Empty,
            AniListId = dto.AniListId,
            AniListOffset = 0,
            AirNotificationsEnabled = false,
            CongaRemindersEnabled = false,
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
                    ProjectId = project.Id,
                    Number = Convert.ToString(i, CultureInfo.InvariantCulture),
                    IsDone = false,
                    AirNotificationPosted = false,
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

        return new Result<CreateProjectResponse>(
            ResultStatus.Success,
            new CreateProjectResponse(project.Id, project.Nickname)
        );
    }

    private static string GetAutoFields(ProjectCreateDto dto)
    {
        return string.Join(
            ", ",
            new[] { nameof(dto.Title), nameof(dto.Length), nameof(dto.Type), nameof(dto.PosterUri) }
                .Zip(new object?[] { dto.Title, dto.Length, dto.Type, dto.PosterUri })
                .Where(p => p.Second is null)
                .Select(p => p.First)
        );
    }
}
