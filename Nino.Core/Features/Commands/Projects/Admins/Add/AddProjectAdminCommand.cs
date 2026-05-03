// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Admins.Add;

public sealed record AddProjectAdminCommand(ProjectId ProjectId, UserId UserId, UserId RequestedBy)
    : ICommand;
