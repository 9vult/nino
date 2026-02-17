// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Dtos;

namespace Nino.Core.Entities;

public abstract class MappedId
{
    [Key]
    public Guid Id { get; set; }
    public ulong DiscordId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public MappedIdDto AsMappedId()
    {
        return new MappedIdDto { Id = Id, DiscordId = DiscordId };
    }
}
