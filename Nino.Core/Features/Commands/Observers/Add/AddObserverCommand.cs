// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Observers.Add;

public sealed record AddObserverCommand(
    ProjectId ProjectId,
    GroupId GroupId,
    UserId RequestedBy,
    bool OverrideVerification,
    ChannelId UpdateChannelId,
    ChannelId ReleaseChannelId,
    RoleId? PrimaryRoleId = null,
    RoleId? SecondaryRoleId = null,
    RoleId? TertiaryRoleId = null
) : ICommand;
