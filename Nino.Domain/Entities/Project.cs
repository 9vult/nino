// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.Entities.Conga;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Project
{
    public ProjectId Id { get; set; } = ProjectId.New();

    public required GroupId GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public required UserId OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public required ProjectType Type { get; set; }

    [MaxLength(32)]
    public required string Nickname { get; set; }

    [MaxLength(128)]
    public required string Title { get; set; }

    [MaxLength(256)]
    public required string PosterUrl { get; set; }

    [MaxLength(256)]
    public string Motd { get; set; } = string.Empty;

    public required AniListId AniListId { get; set; }
    public int AniListOffset { get; set; } = 0;

    public required ChannelId ProjectChannelId { get; set; }
    public Channel ProjectChannel { get; set; } = null!;

    public required ChannelId UpdateChannelId { get; set; }
    public Channel UpdateChannel { get; set; } = null!;

    public required ChannelId ReleaseChannelId { get; set; }
    public Channel ReleaseChannel { get; set; } = null!;

    public required bool IsPrivate { get; set; }
    public bool IsArchived { get; set; } = false;

    public bool AirNotificationEnabled { get; set; } = false;
    public TimeSpan AirNotificationDelay { get; set; } = TimeSpan.Zero;
    public UserId? AirNotificationUserId { get; set; } = null;
    public User? AirNotificationUser { get; set; } = null;
    public MentionRoleId? AirNotificationRoleId { get; set; } = null;
    public MentionRole? AirNotificationRole { get; set; } = null;

    public bool CongaRemindersEnabled { get; set; } = false;
    public TimeSpan CongaReminderPeriod { get; set; } = TimeSpan.Zero;
    public required CongaGraph CongaParticipants { get; set; } = new();

    public ICollection<Alias> Aliases { get; set; } = [];
    public ICollection<Staff> KeyStaff { get; set; } = [];
    public ICollection<Administrator> Administrators { get; set; } = [];
    public ICollection<Episode> Episodes { get; set; } = [];
    public ICollection<Observer> Observers { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
