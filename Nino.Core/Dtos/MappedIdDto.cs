// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Nino.Core.Entities;

namespace Nino.Core.Dtos;

public sealed class MappedIdDto
{
    [JsonIgnore]
    public Guid? Id { get; init; }
    public ulong? DiscordId { get; init; }

    [return: NotNullIfNotNull(nameof(entity))]
    internal static MappedIdDto? FromMappedId(MappedId? entity)
    {
        return entity is null
            ? null
            : new MappedIdDto { Id = entity.Id, DiscordId = entity.DiscordId };
    }
}
