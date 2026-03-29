// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.CongaReminders.Disable;

public sealed record DisableCongaRemindersCommand(ProjectId ProjectId, UserId RequestedBy)
    : ICommand;
