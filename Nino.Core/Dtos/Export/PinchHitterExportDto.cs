// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos.Export;

public sealed class PinchHitterExportDto
{
    public required MappedIdDto UserId { get; set; }
    public required string Abbreviation { get; set; }
}
