// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos.Import;

public sealed class TaskImportDto
{
    public required string Abbreviation { get; init; }
    public required bool IsDone { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastRemindedAt { get; set; }
}
