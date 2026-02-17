// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

/// <summary>
/// A mentionable role, not to be confused with a staff's <see cref="Role"/>
/// </summary>
public class MentionRole : MappedId
{
    /// <inheritdoc />
    public override string ToString()
    {
        return $"R[{Id} ({DiscordId})]";
    }
}
