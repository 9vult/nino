// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Release.Custom;

public sealed record ReleaseCustomCommand(
    ProjectId ProjectId,
    UserId RequestedBy,
    string? Label,
    List<string> Urls,
    RoleId? PrimaryRoleId = null,
    RoleId? SecondaryRoleId = null,
    RoleId? TertiaryRoleId = null,
    string? Commentary = null
) : ReleaseCommandBase(ProjectId, RequestedBy), ICommand;
