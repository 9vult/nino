using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nino.Records.Enums;

namespace Nino.Records;

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
    public string? Motd { get; set; }
    public int? AniListId { get; set; }
    public int? AniListOffset { get; set; }
    public required bool AirReminderEnabled { get; set; }
    public ulong? AirReminderChannelId { get; set; }
    public ulong? AirReminderRoleId { get; set; }
    public ulong? AirReminderUserId { get; set; }
    public required bool CongaReminderEnabled { get; set; }
    public TimeSpan? CongaReminderPeriod { get; set; }
    public ulong? CongaReminderChannelId { get; set; }
    public DateTimeOffset? Created { get; set; }

    public List<string> Aliases { get; set; } = [];
    public List<Staff> KeyStaff { get; set; } = [];
    public List<Administrator> Administrators { get; set; } = [];
    public ICollection<Episode> Episodes = new List<Episode>();
    
    public ICollection<Observer> Observers = new List<Observer>();
        
    /// <summary>
    /// AniList URL. <see langword="null"/> if <see cref="AniListId"/> is <see langword="null"/>.
    /// </summary>
    [NotMapped]
    public string? AniListUrl => AniListId is null ? null : $"https://anilist.co/anime/{AniListId}";

    public override string ToString ()
    {
        return $"P[{Id} ({GuildId}-{Nickname})]";
    }
}