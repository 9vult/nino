using Nino.Records.Enums;

namespace Nino.Records;

public class Project
{
    public required Guid Id { get; set; }
    public required ulong GuildId { get; set; }
    public required string Nickname { get; set; }
    public required string Title { get; set; }
    public required ulong OwnerId { get; set; }
    public required ulong[] AdministratorIds { get; set; }
    public required Staff[] KeyStaff { get; set; }
    public required ProjectType Type { get; set; }
    public required string PosterUri { get; set; }
    public required ulong UpdateChannelId { get; set; }
    public required ulong ReleaseChannelId { get; set; }
    public required bool IsPrivate { get; set; }
    public required bool IsArchived { get; set; }
    public required CongaGraph CongaParticipants { get; set; }
    public required string[] Aliases { get; set; }
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
    
    public string? AniListUrl => AniListId is null ? null : $"https://anilist.co/anime/{AniListId}";

    public override string ToString()
    {
        return $"P[{Id} ({GuildId}-{Nickname})]";
    }
}
