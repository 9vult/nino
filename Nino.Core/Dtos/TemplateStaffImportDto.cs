// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features.Commands.TemplateStaff;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Dtos;

public sealed class TemplateStaffImportDto
{
    public required TemplateStaffApplicator Applicator { get; set; }
    public required Abbreviation Abbreviation { get; set; }
    public required string Name { get; set; }
    public required MappedIdImportDto Assignee { get; set; }
    public required bool IsPseudo { get; set; }
    public decimal? Weight { get; set; }
}
