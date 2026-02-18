// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

namespace Nino.Core.Dtos.Export;

public sealed class StaffExportDto
{
    public required MappedIdDto UserId { get; init; }
    public required bool IsPseudo { get; init; }
    public required string Abbreviation { get; init; }
    public required string Name { get; init; }
    public required decimal Weight { get; init; }

    internal static StaffExportDto FromStaff(Staff staff)
    {
        return new StaffExportDto
        {
            UserId = MappedIdDto.FromMappedId(staff.User),
            IsPseudo = staff.IsPseudo,
            Abbreviation = staff.Role.Abbreviation,
            Name = staff.Role.Name,
            Weight = staff.Role.Weight,
        };
    }
}
