// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Dtos;

public sealed class TaskImportDto
{
    public required Abbreviation Abbreviation { get; set; }
    public required string Name { get; set; }
    public required MappedIdImportDto Assignee { get; set; }
    public required bool IsPseudo { get; set; }
    public decimal? Weight { get; set; }
    public Number First { get; set; }
    public Number? Last { get; set; } = null;
}
