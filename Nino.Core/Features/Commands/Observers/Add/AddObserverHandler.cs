// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Observers.Add;

public sealed class AddObserverHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddObserverHandler> logger
) : ICommandHandler<AddObserverCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(AddObserverCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.User
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var observer = await db.Observers.FirstOrDefaultAsync(o =>
            o.ProjectId == command.ProjectId && o.GroupId == command.GroupId
        );

        if (observer is not null)
        {
            // Proceed only if the user a) made the observer or b) is an admin in the observing group
            verification = await verificationService.VerifyGroupPermissionsAsync(
                command.GroupId,
                command.RequestedBy,
                PermissionsLevel.Administrator
            );
            if (observer.OwnerId != command.RequestedBy && !verification.IsSuccess)
                return Fail(verification.Status);

            logger.LogInformation(
                "Updating group {GroupId}'s observer of project {ProjectId}",
                command.GroupId,
                command.ProjectId
            );

            observer.UpdateChannelId = command.UpdateChannelId;
            observer.ReleaseChannelId = command.ReleaseChannelId;
            observer.PrimaryRoleId = command.PrimaryRoleId;
            observer.SecondaryRoleId = command.SecondaryRoleId;
            observer.TertiaryRoleId = command.TertiaryRoleId;
        }
        else
        {
            logger.LogInformation(
                "Creating observer of project {ProjectId} for group {GroupId}",
                command.ProjectId,
                command.GroupId
            );

            var originGroupId = await db
                .Projects.Where(p => p.Id == command.ProjectId)
                .Select(p => (GroupId?)p.GroupId)
                .FirstOrDefaultAsync();

            if (originGroupId is null)
                throw new NullReferenceException(
                    $"Origin group for project {command.ProjectId} was not found"
                );

            observer = new Observer
            {
                GroupId = command.GroupId,
                OriginGroupId = originGroupId.Value,
                OwnerId = command.RequestedBy,
                ProjectId = command.ProjectId,
                UpdateChannelId = command.UpdateChannelId,
                ReleaseChannelId = command.ReleaseChannelId,
                PrimaryRoleId = command.PrimaryRoleId,
                SecondaryRoleId = command.SecondaryRoleId,
                TertiaryRoleId = command.TertiaryRoleId,
            };
            db.Observers.Add(observer);
        }

        await db.SaveChangesAsync();
        return Success();
    }
}
