// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Task
{
    public TaskId Id { get; set; } = TaskId.New();

    [MaxLength(16)]
    public required string Abbreviation { get; set; }
    public required bool IsDone { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastRemindedAt { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"T[{Id} ({Abbreviation})]";
    }
}
