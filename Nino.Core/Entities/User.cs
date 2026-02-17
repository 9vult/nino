// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

public class User : MappedId
{
    [MaxLength(64)]
    public string? Name { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"U[{Id} ({Name},{DiscordId})]";
    }
}
