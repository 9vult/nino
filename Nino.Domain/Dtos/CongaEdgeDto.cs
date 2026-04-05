// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Dtos;

public sealed class CongaEdgeDto
{
    public required Abbreviation From { get; set; }
    public required Abbreviation To { get; set; }
}
