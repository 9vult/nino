// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos;

public sealed class MappedIdImportDto
{
    public Guid? Id { get; set; } = null;
    public ulong? DiscordId { get; set; } = null;
}
