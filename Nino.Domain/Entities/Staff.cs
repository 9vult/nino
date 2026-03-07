// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Staff
{
    public StaffId Id { get; set; } = StaffId.New();
    public required UserId UserId { get; set; }
    public User User { get; set; } = null!;
    public required Role Role { get; set; }
    public required bool IsPseudo { get; set; }

    // One of these will be set, the other null
    public ProjectId? ProjectId { get; init; }
    public Project? Project { get; init; }

    public EpisodeId? EpisodeId { get; init; }
    public Episode? Episode { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"S[{Id} ({Role.Abbreviation})]";
    }
}
