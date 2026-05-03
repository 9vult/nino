// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Dtos;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Dtos.Export;

public sealed class ProjectExportDto
{
    public required MappedIdDto<GroupId> Group { get; init; }
    public required MappedIdDto<UserId> Owner { get; init; }
    public required ProjectType Type { get; init; }
    public required Alias Nickname { get; init; }
    public required string Title { get; init; }
    public required string PosterUrl { get; init; }
    public required string Motd { get; init; }
    public required AniListId AniListId { get; init; }
    public required int AniListOffset { get; init; }
    public required MappedIdDto<ChannelId> ProjectChannel { get; init; }
    public required MappedIdDto<ChannelId> UpdateChannel { get; init; }
    public required MappedIdDto<ChannelId> ReleaseChannel { get; init; }
    public required bool IsPrivate { get; init; }
    public required bool IsArchived { get; init; }
    public required bool AirNotificationsEnabled { get; init; }
    public required TimeSpan AirNotificationDelay { get; init; }
    public required MappedIdDto<UserId>? AirNotificationUser { get; init; }
    public required MappedIdDto<RoleId>? AirNotificationRole { get; init; }
    public required bool CongaRemindersEnabled { get; init; }
    public required TimeSpan CongaReminderPeriod { get; init; }
    public required CongaGraphDto CongaParticipants { get; init; }
    public required List<Alias> Aliases { get; init; } = [];
    public required List<MappedIdDto<UserId>> Administrators { get; init; }
    public required List<TemplateStaffExportDto> TemplateStaff { get; init; }
    public required List<EpisodeExportDto> Episodes { get; init; }
}
