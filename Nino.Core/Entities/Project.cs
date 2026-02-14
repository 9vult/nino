// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Nino.Core.Enums;

namespace Nino.Core.Entities;

/// <summary>
/// A project
/// </summary>
public class Project
{
    [Key]
    public required Guid Id { get; set; }
    public required ulong GuildId { get; set; }
    public required string Nickname { get; set; }
    public required string Title { get; set; }
    public required ulong OwnerId { get; set; }
    public required ProjectType Type { get; set; }
    public required string PosterUri { get; set; }
    public required ulong UpdateChannelId { get; set; }
    public required ulong ReleaseChannelId { get; set; }
    public required bool IsPrivate { get; set; }
    public required bool IsArchived { get; set; } = false;
    public required CongaGraph CongaParticipants { get; set; } = new();
    public required string Motd { get; set; }
    public required int AniListId { get; set; }
    public required int AniListOffset { get; set; }
    public required bool AirReminderEnabled { get; set; }
    public ulong? AirReminderChannelId { get; set; }
    public ulong? AirReminderRoleId { get; set; }
    public ulong? AirReminderUserId { get; set; }
    public TimeSpan? AirReminderDelay { get; set; }
    public required bool CongaReminderEnabled { get; set; }
    public TimeSpan? CongaReminderPeriod { get; set; }
    public ulong? CongaReminderChannelId { get; set; }
    public required DateTimeOffset Created { get; set; }

    public List<Alias> Aliases { get; set; } = [];
    public List<Staff> KeyStaff { get; set; } = [];
    public List<Administrator> Administrators { get; set; } = [];

    [JsonIgnore]
    public ICollection<Episode> Episodes = new List<Episode>();

    [JsonIgnore]
    public ICollection<Observer> Observers = new List<Observer>();

    /// <summary>
    /// AniList URL.
    /// </summary>
    [NotMapped]
    public string? AniListUrl => $"https://anilist.co/anime/{AniListId}";

    public override string ToString()
    {
        return $"P[{Id} ({GuildId}-{Nickname})]";
    }
}
