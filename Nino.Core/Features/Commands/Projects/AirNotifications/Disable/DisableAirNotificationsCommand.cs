// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.AirNotifications.Disable;

public sealed record DisableAirNotificationsCommand(ProjectId ProjectId, UserId RequestedBy)
    : ICommand;
