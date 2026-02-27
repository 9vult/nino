// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

/// <summary>
/// A server observing a <see cref="Project"/>
/// </summary>
public class Observer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid GroupId { get; set; }
    public required Guid OriginGroupId { get; set; }
    public required Guid OwnerId { get; set; }
    public required Guid ProjectId { get; set; }
    public required bool Blame { get; set; }
    public ulong? RoleId { get; set; }

    [MaxLength(256)]
    public string? ProgressWebhook { get; set; }

    [MaxLength(256)]
    public string? ReleasesWebhook { get; set; }

    public User Owner { get; set; } = null!;
    public Group Group { get; set; } = null!;
    public Group OriginGroup { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
