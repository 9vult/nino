// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Staff
{
    public StaffId Id { get; set; } = StaffId.New();
    public required UserId UserId { get; set; }
    public required Role Role { get; set; }
    public required bool IsPseudo { get; set; }

    public User User { get; set; } = null!;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"S[{Id} ({Role.Abbreviation})]";
    }
}
