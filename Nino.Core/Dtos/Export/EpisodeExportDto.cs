// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Dtos.Export;

public sealed class EpisodeExportDto
{
    public required Number Number { get; init; }
    public required bool IsDone { get; init; }
    public required AirNotificationStatus AirNotificationStatus { get; init; }
    public required List<TaskExportDto> Tasks { get; init; }
}
