// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.AirNotifications.Enable;

public sealed class EnableAirNotificationsHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<EnableAirNotificationsHandler> logger
) : ICommandHandler<EnableAirNotificationsCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(EnableAirNotificationsCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db.Projects.Where(p => p.Id == command.ProjectId).FirstOrDefaultAsync();
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        // Project channel required
        if (project.ProjectChannelId.Value == ChannelId.Unset)
            return Fail(ResultStatus.MissingProjectChannel);

        project.AirNotificationsEnabled = true;
        project.AirNotificationUserId = command.NotificationUserId;
        project.AirNotificationRoleId = command.NotificationRoleId;
        project.AirNotificationDelay = command.Delay;

        logger.LogInformation("Enabling Air Notifications for project {ProjectId}", project.Id);

        await db.SaveChangesAsync();
        return Success();
    }
}
