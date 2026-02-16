// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

/// <summary>
/// A mentionable role, not to be confused with a staff's <see cref="Role"/>
/// </summary>
public class MentionRole
{
    [Key]
    public Guid Id { get; set; }
    public required ulong DiscordId { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"R[{Id} ({DiscordId})]";
    }
}
