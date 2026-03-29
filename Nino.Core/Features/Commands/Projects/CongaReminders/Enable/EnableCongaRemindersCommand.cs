// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.CongaReminders.Enable;

public sealed record EnableCongaRemindersCommand(
    ProjectId ProjectId,
    TimeSpan Period,
    UserId RequestedBy
) : ICommand;
