// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Admins.Remove;

public sealed record RemoveProjectAdminCommand(
    ProjectId ProjectId,
    UserId UserId,
    UserId RequestedBy
) : ICommand;
