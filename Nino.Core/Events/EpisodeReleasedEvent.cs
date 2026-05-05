// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record EpisodeReleasedEvent(
    ProjectId ProjectId,
    Number Number,
    List<string> Urls,
    bool Publish,
    RoleId? PrimaryRoleId = null,
    RoleId? SecondaryRoleId = null,
    RoleId? TertiaryRoleId = null,
    string? Commentary = null
) : IEvent;
