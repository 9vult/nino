// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record EpisodeReleasedEvent(
    ProjectId ProjectId,
    Number Number,
    string Url,
    bool Publish,
    RoleId? PrimaryRoleId = null,
    RoleId? SecondaryRoleId = null,
    RoleId? TertiaryRoleId = null
) : IEvent;
