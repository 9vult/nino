// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

/// <summary>
/// A position
/// </summary>
public class Staff
{
    [Key]
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Role Role { get; set; }
    public required bool IsPseudo { get; set; } = false;

    public User User { get; set; } = null!;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"S[{Id} ({Role.Abbreviation})]";
    }
}
