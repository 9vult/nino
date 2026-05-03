// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Dtos.Export;

public sealed class TaskExportDto
{
    public required MappedIdDto<UserId> Assignee { get; init; }
    public required Abbreviation Abbreviation { get; init; }
    public required string Name { get; init; }
    public required decimal Weight { get; init; }
    public required bool IsPseudo { get; init; }
    public required bool IsDone { get; init; }
    public required DateTimeOffset? UpdatedAt { get; init; }
    public required DateTimeOffset? LastRemindedAt { get; init; }
}
