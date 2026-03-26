// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

/// <summary>
/// A server observing a <see cref="Project"/>
/// </summary>
public sealed class Observer
{
    public ObserverId Id { get; set; } = ObserverId.FromNewGuid();

    public required GroupId GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public required GroupId OriginGroupId { get; set; }
    public Group OriginGroup { get; set; } = null!;

    public required UserId OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public required ProjectId ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ChannelId UpdateChannelId { get; set; }
    public Channel UpdateChannel { get; set; } = null!;

    public ChannelId ReleaseChannelId { get; set; }
    public Channel ReleaseChannel { get; set; } = null!;

    public RoleId? PrimaryRoleId { get; set; }
    public Role? PrimaryRole { get; set; }

    public RoleId? SecondaryRoleId { get; set; }
    public Role? SecondaryRole { get; set; }

    public RoleId? TertiaryRoleId { get; set; }
    public Role? TertiaryRole { get; set; }
}
