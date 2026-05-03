// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Dtos.Export;

public sealed class EpisodeExportDto
{
    public required Number Number { get; init; }
    public required bool IsDone { get; init; }
    public required bool AirNotificationPosted { get; init; }
    public required List<TaskExportDto> Tasks { get; init; }
}
