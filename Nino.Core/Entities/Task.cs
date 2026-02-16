// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

public class Task
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(16)]
    public required string Abbreviation { get; set; }
    public required bool Done { get; set; }
    public DateTimeOffset? Updated { get; set; }
    public DateTimeOffset? LastReminded { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"T[{Id}] ({Abbreviation})]";
    }
}
