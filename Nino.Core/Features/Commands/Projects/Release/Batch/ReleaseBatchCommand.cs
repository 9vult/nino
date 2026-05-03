// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Release.Batch;

public sealed record ReleaseBatchCommand(
    ProjectId ProjectId,
    UserId RequestedBy,
    Number FirstNumber,
    Number LastNumber,
    List<string> Urls,
    RoleId? PrimaryRoleId = null,
    RoleId? SecondaryRoleId = null,
    RoleId? TertiaryRoleId = null
) : ReleaseCommandBase(ProjectId, RequestedBy), ICommand;
