// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos.Import;

public sealed class EpisodeImportDto
{
    public required string Number { get; init; }
    public required bool IsDone { get; init; }

    public required StaffImportDto[] AdditionalStaff { get; set; }
    public required TaskImportDto[] Tasks { get; set; }

    public bool AirNotificationPosted { get; init; } = false;
    public DateTimeOffset? UpdatedAt { get; init; } = null;
    public PinchHitterImportDto[] PinchHitters { get; set; } = [];
}
