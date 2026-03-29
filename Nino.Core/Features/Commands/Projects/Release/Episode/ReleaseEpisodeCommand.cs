// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Release.Episode;

public sealed record ReleaseEpisodeCommand(
    ProjectId ProjectId, 
    UserId RequestedBy, 
    Number Number,
    string Url,
    RoleId? PrimaryRoleId = null,
    RoleId? SecondaryRoleId = null,
    RoleId? TertiaryRoleId = null
) : ICommand;
