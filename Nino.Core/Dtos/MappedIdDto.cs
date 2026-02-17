// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Nino.Core.Dtos;

public sealed class MappedIdDto
{
    [JsonIgnore]
    public Guid? Id { get; init; }
    public ulong? DiscordId { get; init; }
}
