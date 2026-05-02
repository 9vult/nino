// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Commands.Projects.Create.CreateProjectResponse>;

namespace Nino.Core.Features.Commands.Projects.Create;

public sealed class CreateProjectHandler(
    NinoDbContext db,
    IAniListService aniListService,
    IUserVerificationService verificationService,
    ILogger<CreateProjectHandler> logger
) : ICommandHandler<CreateProjectCommand, Result<CreateProjectResponse>>
{
    private const string FallbackPosterUrl = "https://files.catbox.moe/j3qizm.png";

    /// <inheritdoc />
    public async Task<Result<CreateProjectResponse>> HandleAsync(CreateProjectCommand command)
    {
        // Check permissions
        // Override means the user is authorized through the provider (e.g. Discord Admin)
        var verification = await verificationService.VerifyGroupPermissionsAsync(
            command.GroupId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!command.OverrideVerification && !verification.IsSuccess)
            return Fail(verification.Status);

        if (
            await db.Projects.AnyAsync(p =>
                p.GroupId == command.GroupId && p.Nickname == command.Nickname
            )
        )
            return Fail(ResultStatus.ProjectConflict);

        var autoFields = GetAutoFields(command);
        if (!string.IsNullOrEmpty(autoFields))
        {
            logger.LogInformation(
                "AniList will be used in the construction of project {Nickname} for {AutoFields}",
                command.Nickname,
                autoFields
            );
        }

        var animeLookup = await aniListService.GetAnimeAsync(command.AniListId);

        // Bad Request is used for ID <= 0, so ignore that
        if (animeLookup is { IsSuccess: false, Status: not ResultStatus.BadRequest })
            return Fail(ResultStatus.Unauthorized);

        if (animeLookup.IsSuccess)
        {
            var anime = animeLookup.Value!;
            command = command with
            {
                Title = command.Title ?? anime.Title,
                Length = command.Length ?? anime.EpisodeCount,
                Type = command.Type ?? anime.Type,
                PosterUrl = command.PosterUrl ?? anime.PosterUrl ?? FallbackPosterUrl,
            };
        }

        if (
            command.Title is null
            || command.Length is null
            || command.Type is null
            || command.PosterUrl is null
        )
        {
            var missingFields = GetAutoFields(command);
            logger.LogWarning(
                "Failed to fetch the following fields from AniList: {MissingFields}",
                missingFields
            );
            return Fail(ResultStatus.BadRequest, message: missingFields);
        }

        // Truncate the title if too long

        var truncTitle =
            command.Title.Length <= Length.Title
                ? command.Title
                : command.Title[..(Length.Title - 3)] + "...";

        var project = new Project
        {
            GroupId = command.GroupId,
            OwnerId = command.RequestedBy,
            Type = command.Type.Value,
            Nickname = command.Nickname,
            Title = truncTitle,
            PosterUrl = command.PosterUrl,
            AniListId = command.AniListId,
            ProjectChannelId = command.ProjectChannelId,
            UpdateChannelId = command.UpdateChannelId,
            ReleaseChannelId = command.ReleaseChannelId,
            IsPrivate = command.IsPrivate,
        };

        logger.LogInformation("Creating project {Project}", project);

        var episodes = new List<Episode>(command.Length.Value);
        for (var i = command.FirstEpisode; i < command.FirstEpisode + command.Length.Value; i++)
        {
            var episode = new Episode
            {
                ProjectId = project.Id,
                GroupId = command.GroupId,
                Number = Number.From(Convert.ToString(i, CultureInfo.InvariantCulture)),
                IsDone = false,
            };
            episodes.Add(episode);
        }

        logger.LogInformation(
            "Creating {EpisodeCount} episodes for project {ProjectId}",
            episodes.Count,
            project.Id
        );

        await db.Projects.AddAsync(project);
        await db.Episodes.AddRangeAsync(episodes);
        await db.SaveChangesAsync();

        return Success(new CreateProjectResponse(project.Id));
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
