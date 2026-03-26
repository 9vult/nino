// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Task
{
    public TaskId Id { get; set; } = TaskId.FromNewGuid();

    public required EpisodeId EpisodeId { get; set; }
    public Episode Episode { get; set; } = null!;

    [MaxLength(Length.Abbreviation)]
    public required Abbreviation Abbreviation { get; set; }

    [MaxLength(Length.RoleName)]
    public required string Name { get; set; }

    public required decimal Weight { get; set; }

    public required bool IsPseudo { get; set; }

    public required bool IsDone { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastRemindedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
