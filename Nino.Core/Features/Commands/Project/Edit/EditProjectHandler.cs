// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.Project.Edit;

public sealed class EditProjectHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<EditProjectHandler> logger
)
{
    public async Task<Result<GenericResponse>> HandleAsync(EditProjectCommand input)
    {
        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                input.ProjectId,
                input.RequestedBy,
                PermissionsLevel.Administrator
            )
        )
            return Result<GenericResponse>.Fail(ResultStatus.Unauthorized);

        var project = await db.Projects.SingleAsync(p => p.Id == input.ProjectId);

        if (input.Nickname is not null)
        {
            logger.LogTrace("Renaming {Project} to {Nickname}", project, input.Nickname);
            project.Nickname = input.Nickname;
        }

        if (input.Title is not null)
        {
            logger.LogTrace("Changing title of {Project} to {Title}", project, input.Title);
            project.Title = input.Title;
        }

        if (input.PosterUrl is not null)
        {
            logger.LogTrace("Changing {Project}'s poster url", project);
            project.PosterUrl = input.PosterUrl;
        }

        if (input.Motd is not null)
        {
            logger.LogTrace("Changing {Project}'s MOTD to '{Motd}'", project, input.Motd);
            project.Motd = input.Motd;
        }

        if (input.AniListId is not null)
        {
            logger.LogTrace(
                "Setting {Project}'s AniListID to {AniListId}",
                project,
                input.AniListId
            );
            project.AniListId = input.AniListId.Value;
        }

        if (input.AniListOffset is not null)
        {
            logger.LogTrace(
                "Setting {Project}'s AniListOffset to {AniListOffset}",
                project,
                input.AniListOffset
            );
            project.AniListOffset = input.AniListOffset.Value;
        }

        if (input.IsPrivate is not null)
        {
            logger.LogTrace(
                "Setting {Project}'s IsPrivate to {IsPrivate}",
                project,
                input.IsPrivate
            );
            project.IsPrivate = input.IsPrivate.Value;
        }

        if (input.ProjectChannelId is not null)
        {
            logger.LogTrace(
                "Setting {Project}'s ProjectChannel to {ProjectChannelId}",
                project,
                input.ProjectChannelId
            );
            project.ProjectChannelId = input.ProjectChannelId.Value;
        }

        if (input.UpdateChannelId is not null)
        {
            logger.LogTrace(
                "Setting {Project}'s UpdateChannel to {UpdateChannelId}",
                project,
                input.UpdateChannelId
            );
            project.ProjectChannelId = input.UpdateChannelId.Value;
        }

        if (input.ReleaseChannelId is not null)
        {
            logger.LogTrace(
                "Setting {Project}'s ReleaseChannel to {ReleaseChannelId}",
                project,
                input.ReleaseChannelId
            );
            project.ReleaseChannelId = input.ReleaseChannelId.Value;
        }

        await db.SaveChangesAsync();
        return Result<GenericResponse>.Success(
            new GenericResponse(project.Title, project.Type, project.PosterUrl)
        );
    }
}
