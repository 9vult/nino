// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(64)]
    public required string Name { get; set; }
    public required ulong DiscordId { get; set; }
}
