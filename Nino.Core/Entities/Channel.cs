// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

public class Channel : MappedId
{
    public Guid GroupId { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"C[{Id} (#{DiscordId})]";
    }
}
