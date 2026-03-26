// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Episode
{
    public EpisodeId Id { get; set; } = EpisodeId.FromNewGuid();

    public required ProjectId ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public required GroupId GroupId { get; set; }
    public Group Group { get; set; } = null!;

    [MaxLength(Length.Number)]
    public required Number Number { get; set; }

    public required bool IsDone { get; set; }

    public bool AirNotificationPosted { get; set; } = false;

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Task> Tasks { get; set; } = [];
}
