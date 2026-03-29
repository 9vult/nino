// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Edit;

public sealed class EditProjectHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<EditProjectHandler> logger
) : ICommandHandler<EditProjectCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(EditProjectCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == command.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        if (command.Nickname is not null)
        {
            logger.LogTrace(
                "Renaming project {ProjectId} to {Nickname}",
                project.Id,
                command.Nickname
            );
            project.Nickname = command.Nickname.Value;
        }

        if (command.Title is not null)
        {
            logger.LogTrace(
                "Changing title of project {ProjectId} to {Title}",
                project.Id,
                command.Title
            );
            project.Title = command.Title;
        }

        if (command.PosterUrl is not null)
        {
            logger.LogTrace("Changing project {ProjectId}'s poster url", project.Id);
            project.PosterUrl = command.PosterUrl;
        }

        if (command.Motd is not null)
        {
            logger.LogTrace(
                "Changing project {ProjectId}'s MOTD to '{Motd}'",
                project.Id,
                command.Motd
            );
            project.Motd = command.Motd;
        }

        if (command.AniListId is not null)
        {
            logger.LogTrace(
                "Setting project {ProjectId}'s AniListID to {AniListId}",
                project.Id,
                command.AniListId
            );
            project.AniListId = command.AniListId.Value;
        }

        if (command.AniListOffset is not null)
        {
            logger.LogTrace(
                "Setting project {ProjectId}'s AniListOffset to {AniListOffset}",
                project.Id,
                command.AniListOffset
            );
            project.AniListOffset = command.AniListOffset.Value;
        }

        if (command.IsPrivate is not null)
        {
            logger.LogTrace(
                "Setting project {ProjectId}'s IsPrivate to {IsPrivate}",
                project.Id,
                command.IsPrivate
            );
            project.IsPrivate = command.IsPrivate.Value;
        }

        if (command.ProjectChannelId is not null)
        {
            logger.LogTrace(
                "Setting project {ProjectId}'s ProjectChannel to {ProjectChannelId}",
                project.Id,
                command.ProjectChannelId
            );
            project.ProjectChannelId = command.ProjectChannelId.Value;
        }

        if (command.UpdateChannelId is not null)
        {
            logger.LogTrace(
                "Setting project {ProjectId}'s UpdateChannel to {UpdateChannelId}",
                project.Id,
                command.UpdateChannelId
            );
            project.ProjectChannelId = command.UpdateChannelId.Value;
        }

        if (command.ReleaseChannelId is not null)
        {
            logger.LogTrace(
                "Setting project {ProjectId}'s ReleaseChannel to {ReleaseChannelId}",
                project.Id,
                command.ReleaseChannelId
            );
            project.ReleaseChannelId = command.ReleaseChannelId.Value;
        }

        await db.SaveChangesAsync();
        return Success();
    }
}
