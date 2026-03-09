// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Entities;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.Project.Create;

public sealed class CreateProjectHandler(
    NinoDbContext db,
    IAniListService aniListService,
    IUserVerificationService verificationService,
    ILogger<CreateProjectHandler> logger
)
{
    private const string FallbackPosterUrl = "https://files.catbox.moe/j3qizm.png";

    public async Task<Result<CreateProjectResponse>> HandleAsync(CreateProjectCommand input)
    {
        var (groupId, ownerId, overrideVerification) = input;

        // Check permissions
        // Override means the user is authorized through the provider (e.g. Discord Admin)
        if (
            !overrideVerification
            && !await verificationService.VerifyGroupPermissionsAsync(
                groupId,
                ownerId,
                PermissionsLevel.Administrator
            )
        )
        {
            return Result<CreateProjectResponse>.Fail(ResultStatus.Unauthorized);
        }

        // Sanitize inputs
        input.Nickname = input.Nickname.Trim().ToLowerInvariant().Replace(" ", string.Empty);
        input.Title = input.Title?.Trim();

        if (await db.Projects.AnyAsync(p => p.GroupId == groupId && p.Nickname == input.Nickname))
            return Result<CreateProjectResponse>.Fail(ResultStatus.Conflict);

        var autoFields = GetAutoFields(input);
        if (!string.IsNullOrEmpty(autoFields))
        {
            logger.LogInformation(
                "AniList will be used in the construction of project {Nickname} for {AutoFields}",
                input.Nickname,
                autoFields
            );
        }

        var animeLookup = await aniListService.GetAnimeAsync(input.AniListId);

        // Bad Request is used for ID <= 0, so ignore that
        if (animeLookup is { IsSuccess: false, Status: not ResultStatus.BadRequest })
            return Result<CreateProjectResponse>.Fail(ResultStatus.Unauthorized);

        if (animeLookup.IsSuccess)
        {
            var anime = animeLookup.Value!;
            input.Title ??= anime.Title;
            input.Length ??= anime.EpisodeCount;
            input.Type ??= anime.Type;
            input.PosterUrl ??= anime.PosterUrl ?? FallbackPosterUrl;
        }

        if (
            input.Title is null
            || input.Length is null
            || input.Type is null
            || input.PosterUrl is null
        )
            return Result<CreateProjectResponse>.Fail(ResultStatus.BadRequest);

        var project = new Domain.Entities.Project
        {
            GroupId = groupId,
            OwnerId = ownerId,
            Type = input.Type.Value,
            Nickname = input.Nickname,
            Title = input.Title,
            PosterUrl = input.PosterUrl,
            AniListId = input.AniListId,
            ProjectChannelId = input.ProjectChannelId,
            UpdateChannelId = input.UpdateChannelId,
            ReleaseChannelId = input.ReleaseChannelId,
            IsPrivate = input.IsPrivate,
        };

        logger.LogInformation("Creating project {Project}", project);

        var episodes = new List<Episode>(input.Length.Value);
        for (var i = input.FirstEpisode; i < input.FirstEpisode + input.Length.Value; i++)
        {
            var episode = new Episode
            {
                ProjectId = project.Id,
                GroupId = groupId,
                Number = Convert.ToString(i, CultureInfo.InvariantCulture),
                IsDone = false,
            };
            episodes.Add(episode);
        }

        logger.LogInformation(
            "Creating {EpisodeCount} episodes for project {ProjectId}",
            episodes.Count,
            project.Id
        );

        await db.Episodes.AddRangeAsync(episodes);
        await db.SaveChangesAsync();

        // Basically return the AniList fields back to the caller
        return Result<CreateProjectResponse>.Success(
            new CreateProjectResponse(
                project.Id,
                project.Nickname,
                project.Title,
                episodes.Count,
                project.Type
            )
        );
    }

    /// <summary>
    /// Get the fields that will be automatically populated by AniList
    /// </summary>
    /// <param name="dto">Create command</param>
    /// <returns>String containing a list of field names</returns>
    private static string GetAutoFields(CreateProjectCommand dto)
    {
        return string.Join(
            ", ",
            new[] { nameof(dto.Title), nameof(dto.Length), nameof(dto.Type), nameof(dto.PosterUrl) }
                .Zip(new object?[] { dto.Title, dto.Length, dto.Type, dto.PosterUrl })
                .Where(p => p.Second is null)
                .Select(p => p.First)
        );
    }
}
