// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
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

    internal static ProjectExportDto FromProject(Project project)
    {
        return new ProjectExportDto
        {
            GroupId = MappedIdDto.FromMappedId(project.Group),
            OwnerId = MappedIdDto.FromMappedId(project.Owner),
            Nickname = project.Nickname,
            Title = project.Title,
            Type = project.Type,
            PosterUri = project.PosterUri,
            IsPrivate = project.IsPrivate,
            IsArchived = project.IsArchived,
            ProjectChannelId = MappedIdDto.FromMappedId(project.ProjectChannel),
            UpdateChannelId = MappedIdDto.FromMappedId(project.UpdateChannel),
            ReleaseChannelId = MappedIdDto.FromMappedId(project.ReleaseChannel),
            Motd = project.Motd,
            Aliases = project.Aliases.Select(a => a.Value).ToArray(),
            AniListId = project.AniListId,
            AniListOffset = project.AniListOffset,
            AirNotificationsEnabled = project.AirNotificationsEnabled,
            CongaRemindersEnabled = project.CongaRemindersEnabled,
            AirNotificationDelay = project.AirNotificationDelay,
            CongaReminderPeriod = project.CongaReminderPeriod,
            AirNotificationUserId = MappedIdDto.FromMappedId(project.AirNotificationUser),
            AirNotificationRoleId = MappedIdDto.FromMappedId(project.AirNotificationRole),
            KeyStaff = project.KeyStaff.Select(StaffExportDto.FromStaff).ToArray(),
            CongaParticipants = project.CongaParticipants.Serialize(),
            Administrators = project
                .Administrators.Select(a => MappedIdDto.FromMappedId(a.User))
                .ToArray(),
        };
    }
}
