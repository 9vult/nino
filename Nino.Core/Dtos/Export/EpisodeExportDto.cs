// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos.Export;

public sealed class EpisodeExportDto
{
    public required string Number { get; init; }

    public required bool IsDone { get; init; }
    public required bool AirNotificationPosted { get; init; }
    public required DateTimeOffset? UpdatedAt { get; init; }

    public required StaffExportDto[] AdditionalStaff { get; set; }
    public required TaskExportDto[] Tasks { get; set; }
    public required PinchHitterExportDto[] PinchHitters { get; set; }
}
