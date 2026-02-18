// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

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

    internal static EpisodeExportDto FromEpisode(Episode episode)
    {
        return new EpisodeExportDto
        {
            Number = episode.Number,
            IsDone = episode.IsDone,
            AirNotificationPosted = episode.AirNotificationPosted,
            UpdatedAt = episode.UpdatedAt,
            AdditionalStaff = episode.AdditionalStaff.Select(StaffExportDto.FromStaff).ToArray(),
            Tasks = episode.Tasks.Select(TaskExportDto.FromTask).ToArray(),
            PinchHitters = episode
                .PinchHitters.Select(PinchHitterExportDto.FromPinchHitter)
                .ToArray(),
        };
    }
}
