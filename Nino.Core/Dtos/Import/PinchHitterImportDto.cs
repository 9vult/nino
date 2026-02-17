// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos.Import;

public sealed class PinchHitterImportDto
{
    public required MappedIdDto UserId { get; set; }
    public required string Abbreviation { get; set; }
}
