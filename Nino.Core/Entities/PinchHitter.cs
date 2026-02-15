// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

/// <summary>
/// Replaces a <see cref="Staff"/> position for a single episode
/// </summary>
public class PinchHitter
{
    [Key]
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }

    [MaxLength(16)]
    public required string Abbreviation { get; set; }

    public User User { get; set; } = null!;
}
