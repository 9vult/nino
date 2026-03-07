// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public abstract class MappedId<TId>
    where TId : struct, IId<TId>
{
    public TId Id { get; set; } = TId.New();
    public ulong? DiscordId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
