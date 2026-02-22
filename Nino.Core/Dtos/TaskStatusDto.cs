// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos;

public sealed class TaskStatusDto
{
    public required string Abbreviation { get; init; }
    public required decimal Weight { get; init; }
    public required MappedIdDto User { get; init; }
    public required bool IsPseudo { get; init; }
    public required bool IsDone { get; init; }
}
