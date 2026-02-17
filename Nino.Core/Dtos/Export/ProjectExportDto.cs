// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Dtos.Export;

public sealed class ProjectExportDto
{
    public required MappedIdDto GroupId { get; init; }
    public required MappedIdDto OwnerId { get; init; }

    public required string Nickname { get; init; }
    public required string Title { get; init; }
    public required ProjectType Type { get; init; }
    public required string PosterUri { get; init; }
    public required bool IsPrivate { get; init; }
    public required bool IsArchived { get; init; }

    public required MappedIdDto ProjectChannelId { get; init; }
    public required MappedIdDto UpdateChannelId { get; init; }
    public required MappedIdDto ReleaseChannelId { get; init; }

    public required string Motd { get; init; }
    public required string[] Aliases { get; init; }

    public required int AniListId { get; init; }
    public required int AniListOffset { get; init; }

    public required bool AirNotificationsEnabled { get; init; }
    public required bool CongaRemindersEnabled { get; init; }
    public required TimeSpan AirNotificationDelay { get; init; }
    public required TimeSpan CongaReminderPeriod { get; init; }
    public required MappedIdDto? AirNotificationUserId { get; init; }
    public required MappedIdDto? AirNotificationRoleId { get; init; }

    public required StaffExportDto[] KeyStaff { get; init; }
    public required CongaNodeDto[] CongaParticipants { get; init; }
    public required MappedIdDto[] Administrators { get; init; }
}
