// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Entities;

public abstract class MappedId<TId>
    where TId : IVogen<TId, Guid>
{
    public TId Id { get; set; } = TId.From(Guid.NewGuid());
    public ulong? DiscordId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is MappedId<TId> other)
            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        return false;
    }

    protected bool Equals(MappedId<TId> other)
    {
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }
}
