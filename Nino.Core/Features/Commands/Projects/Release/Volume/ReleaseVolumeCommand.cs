// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Release.Volume;

public sealed record ReleaseVolumeCommand(
    ProjectId ProjectId,
    UserId RequestedBy,
    Number Number,
    string Url,
    RoleId? PrimaryRoleId = null,
    RoleId? SecondaryRoleId = null,
    RoleId? TertiaryRoleId = null
) : ICommand;
