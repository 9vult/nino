// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class MentionRole : MappedId<MentionRoleId>
{
    /// <inheritdoc />
    public override string ToString()
    {
        return $"R[{Id} (D{DiscordId})]";
    }
}
