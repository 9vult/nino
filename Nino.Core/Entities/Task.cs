// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

public class Task
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(16)]
    public required string Abbreviation { get; set; }
    public required bool IsDone { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastRemindedAt { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"T[{Id}] ({Abbreviation})]";
    }
}
