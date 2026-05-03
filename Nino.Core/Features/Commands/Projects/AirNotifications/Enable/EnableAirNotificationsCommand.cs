// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.AirNotifications.Enable;

public sealed record EnableAirNotificationsCommand(
    ProjectId ProjectId,
    UserId RequestedBy,
    UserId? NotificationUserId,
    RoleId? NotificationRoleId,
    TimeSpan Delay
) : ICommand;
