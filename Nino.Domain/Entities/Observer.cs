// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

/// <summary>
/// A server observing a <see cref="Project"/>
/// </summary>
public sealed class Observer
{
    public ObserverId Id { get; set; } = ObserverId.New();

    public required GroupId GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public required GroupId OriginGroupId { get; set; }
    public Group OriginGroup { get; set; } = null!;

    public required UserId OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public required ProjectId ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public required bool Blame { get; set; }

    public MentionRoleId? RoleId { get; set; }
    public MentionRole? Role { get; set; }

    [MaxLength(256)]
    public string? ProgressWebhook { get; set; }

    [MaxLength(256)]
    public string? ReleasesWebhook { get; set; }
}
