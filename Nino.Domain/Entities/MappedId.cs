// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Entities;

public abstract class MappedId<TId>
    where TId : IVogen<TId, Guid>
{
    public TId Id { get; set; } = TId.From(Guid.NewGuid());
    public ulong? DiscordId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
