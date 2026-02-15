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
    public required Guid Id { get; set; }
    public required Guid GroupId { get; set; }

    [MaxLength(32)]
    public required string Nickname { get; set; }

    [MaxLength(128)]
    public required string Title { get; set; }
    public required Guid OwnerId { get; set; }
    public required ProjectType Type { get; set; }

    [MaxLength(256)]
    public required string PosterUri { get; set; }
    public required Guid UpdateChannelId { get; set; }
    public required Guid ReleaseChannelId { get; set; }
    public required bool IsPrivate { get; set; }
    public required bool IsArchived { get; set; }
    public required CongaGraph CongaParticipants { get; set; } = new();

    [MaxLength(256)]
    public required string Motd { get; set; }
    public required int AniListId { get; set; }
    public required int AniListOffset { get; set; }
    public required bool AirReminderEnabled { get; set; }
    public required bool CongaReminderEnabled { get; set; }
    public required DateTimeOffset Created { get; set; }

    public Guid? AirReminderChannelId { get; set; }
    public Guid? AirReminderUserId { get; set; }
    public ulong? AirReminderRoleId { get; set; }
    public TimeSpan? AirReminderDelay { get; set; }
    public TimeSpan? CongaReminderPeriod { get; set; }
    public Guid? CongaReminderChannelId { get; set; }

    public ICollection<Episode> Episodes { get; set; } = [];
    public ICollection<Observer> Observers { get; set; } = [];

    public Group Group { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public Channel UpdateChannel { get; set; } = null!;
    public Channel ReleaseChannel { get; set; } = null!;
    public Channel? AirReminderChannel { get; set; }
    public Channel? CongaReminderChannel { get; set; }

    public ICollection<Alias> Aliases { get; set; } = [];
    public ICollection<Staff> KeyStaff { get; set; } = [];
    public ICollection<Administrator> Administrators { get; set; } = [];

    /// <summary>
    /// AniList URL.
    /// </summary>
    [NotMapped]
    public string AniListUrl => $"https://anilist.co/anime/{AniListId}";

    /// <inheritdoc />
    public override string ToString()
    {
        return $"P[{Id} ({Nickname})]";
    }
}
