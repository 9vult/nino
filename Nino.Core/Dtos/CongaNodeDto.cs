// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Dtos;

public sealed class CongaNodeDto
{
    public required string Abbreviation { get; init; }
    public required CongaNodeType Type { get; init; } = CongaNodeType.KeyStaff;
    public required string[] Dependents { get; init; } = [];
}
