// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

public class Channel
{
    [Key]
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public required ulong DiscordId { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"C[{Id} (#{DiscordId})]";
    }
}
