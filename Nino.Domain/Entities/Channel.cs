// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Channel : MappedId<ChannelId>
{
    /// <inheritdoc />
    public override string ToString()
    {
        return $"C[{Id} (D{DiscordId})]";
    }
}
