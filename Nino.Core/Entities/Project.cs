// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations.Schema;
using Nino.Core.Enums;

namespace Nino.Core.Entities;

/// <summary>
/// A project
/// </summary>
public class Project
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid GroupId { get; set; }

    [MaxLength(32)]
    public required string Nickname { get; set; }

    [MaxLength(128)]
    public required string Title { get; set; }
    public required Guid OwnerId { get; set; }
    public required ProjectType Type { get; set; }

    [MaxLength(256)]
    public required string PosterUrl { get; set; }
    public required Guid ProjectChannelId { get; set; }
    public required Guid UpdateChannelId { get; set; }
    public required Guid ReleaseChannelId { get; set; }
    public required bool IsPrivate { get; set; }
    public required bool IsArchived { get; set; }
    public required CongaGraph CongaParticipants { get; set; } = new();

    [MaxLength(256)]
    public required string Motd { get; set; }
    public required int AniListId { get; set; }
    public required int AniListOffset { get; set; }
    public required bool AirNotificationsEnabled { get; set; }
    public required bool CongaRemindersEnabled { get; set; }
    public required DateTimeOffset Created { get; set; }

    public Guid? AirNotificationUserId { get; set; }
    public Guid? AirNotificationRoleId { get; set; }
    public TimeSpan AirNotificationDelay { get; set; } = TimeSpan.Zero;
    public TimeSpan CongaReminderPeriod { get; set; } = TimeSpan.Zero;

    public ICollection<Episode> Episodes { get; set; } = [];
    public ICollection<Observer> Observers { get; set; } = [];

    public Group Group { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public Channel ProjectChannel { get; set; } = null!;
    public Channel UpdateChannel { get; set; } = null!;
    public Channel ReleaseChannel { get; set; } = null!;
    public Channel? AirNotificationChannel { get; set; }
    public User? AirNotificationUser { get; set; }
    public MentionRole? AirNotificationRole { get; set; }

    public ICollection<Alias> Aliases { get; set; } = [];
    public ICollection<Staff> KeyStaff { get; set; } = [];
    public ICollection<Administrator> Administrators { get; set; } = [];

    /// <summary>
    /// AniList URL.
    /// </summary>
    [NotMapped]
    public string AniListUrl => $"https://anilist.co/anime/{AniListId}";

    /// <summary>
    /// Helper for getting all staff for an episode
    /// </summary>
    /// <param name="episode">Episode to fetch additional staff from</param>
    /// <returns>Combined Key and Additional staff</returns>
    public IEnumerable<Staff> GetCombinedStaff(Episode episode)
    {
        return KeyStaff.Concat(episode.AdditionalStaff);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"P[{Id} ({Nickname})]";
    }
}
