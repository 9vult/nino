// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

namespace Nino.Core.Dtos.Export;

public sealed class PinchHitterExportDto
{
    public required MappedIdDto UserId { get; set; }
    public required string Abbreviation { get; set; }

    internal static PinchHitterExportDto FromPinchHitter(PinchHitter pinchHitter)
    {
        return new PinchHitterExportDto
        {
            UserId = MappedIdDto.FromMappedId(pinchHitter.User),
            Abbreviation = pinchHitter.Abbreviation,
        };
    }
}
