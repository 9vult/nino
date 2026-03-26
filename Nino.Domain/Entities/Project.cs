// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Channels;
using Nino.Domain.Entities.Conga;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Project
{
    public ProjectId Id { get; set; } = ProjectId.FromNewGuid();

    public required GroupId GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public required UserId OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public required ProjectType Type { get; set; }

    [MaxLength(Length.Alias)]
    public required Alias Nickname { get; set; }

    [MaxLength(Length.Title)]
    public required string Title { get; set; }

    [MaxLength(Length.PosterUrl)]
    public required string PosterUrl { get; set; }

    [MaxLength(Length.Motd)]
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

    public bool AirNotificationsEnabled { get; set; } = false;
    public TimeSpan AirNotificationDelay { get; set; } = TimeSpan.Zero;
    public UserId? AirNotificationUserId { get; set; } = null;
    public User? AirNotificationUser { get; set; } = null;
    public RoleId? AirNotificationRoleId { get; set; } = null;
    public RoleId? AirNotificationRole { get; set; } = null;

    public bool CongaRemindersEnabled { get; set; } = false;

    public TimeSpan CongaReminderPeriod { get; set; } = TimeSpan.Zero;
    public CongaGraph CongaParticipants { get; set; } = new();

    public ObserverId? DelegateObserverId { get; set; }
    public Observer? DelegateObserver { get; set; } = null;

    public ICollection<ProjectAlias> Aliases { get; set; } = [];
    public ICollection<TemplateStaff> TemplateStaff { get; set; } = [];
    public ICollection<Administrator> Administrators { get; set; } = [];
    public ICollection<Episode> Episodes { get; set; } = [];
    public ICollection<Observer> Observers { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [NotMapped]
    public string AniListUrl =>
        AniListId.Value > 0 ? $"https://anilist.co/anime/{AniListId}" : string.Empty;
}
