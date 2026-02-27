// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Dtos.Import;

public sealed class ProjectImportDto
{
    public required MappedIdDto Group { get; init; }
    public required MappedIdDto Owner { get; init; }

    public required string Nickname { get; init; }
    public required string Title { get; init; }
    public required ProjectType Type { get; init; }
    public required string PosterUrl { get; init; }
    public required bool IsPrivate { get; init; }
    public required bool IsArchived { get; init; }

    public required MappedIdDto ProjectChannel { get; init; }
    public required MappedIdDto UpdateChannel { get; init; }
    public required MappedIdDto ReleaseChannel { get; init; }

    public required int AniListId { get; init; }

    public required StaffImportDto[] KeyStaff { get; init; }
    public required CongaNodeDto[] CongaParticipants { get; init; }
    public required MappedIdDto[] Administrators { get; init; }

    public int AniListOffset { get; init; } = 0;
    public bool AirNotificationsEnabled { get; init; } = false;
    public bool CongaRemindersEnabled { get; init; } = false;
    public TimeSpan AirNotificationDelay { get; init; } = TimeSpan.Zero;
    public TimeSpan CongaReminderPeriod { get; init; } = TimeSpan.FromDays(7);
}
