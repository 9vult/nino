// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos;

public sealed class EpisodeStatusDto
{
    public required string Number { get; init; }
    public required bool IsDone { get; init; }
    public required TaskStatusDto[] Tasks { get; init; }
    public required DateTimeOffset? UpdatedAt { get; init; }
}
