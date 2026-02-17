// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos.Export;

public sealed class StaffExportDto
{
    public required MappedIdDto UserId { get; init; }
    public required bool IsPseudo { get; init; }
    public required string Abbreviation { get; init; }
    public required string Name { get; init; }
    public required decimal Weight { get; init; }
}
