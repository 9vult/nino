// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;

namespace Nino.Domain.Dtos;

public sealed record CongaNodeDto
{
    public required string Abbreviation { get; init; }
    public required CongaNodeType Type { get; init; } = CongaNodeType.KeyStaff;
    public required string[] Dependents { get; init; } = [];
}
