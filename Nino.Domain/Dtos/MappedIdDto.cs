// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using Nino.Domain.Entities;

namespace Nino.Domain.Dtos;

public sealed record MappedIdDto<TId>(TId Id, ulong? DiscordId)
    where TId : struct, IVogen<TId, Guid>
{
    [return: NotNullIfNotNull(nameof(entity))]
    public static MappedIdDto<TId>? From(MappedId<TId>? entity) =>
        entity is not null ? new MappedIdDto<TId>(entity.Id, entity.DiscordId) : null;
}
