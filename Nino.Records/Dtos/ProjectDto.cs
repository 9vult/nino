using Nino.Records.Enums;

namespace Nino.Records.Dtos;

public record ProjectDto
{
    public required Guid Id { get; set; }
    public required string GuildId { get; set; }
    public required string Nickname { get; set; }
    public required string Title { get; set; }
    
    public required string OwnerId { get; set; }
    public required List<string> AdministratorIds { get; set; }
    public required List<StaffDto> KeyStaff { get; set; }
    
    public required ProjectType Type { get; set; }
    public required string PosterUri { get; set; }
    public required string UpdateChannelId { get; set; }
    public required string ReleaseChannelId { get; set; }
    public required bool IsPrivate { get; set; }
    public required bool IsArchived { get; set; }
    public required List<CongaNodeDto> CongaParticipants { get; set; }
    public required List<string> Aliases { get; set; }
    public string? Motd { get; set; }
    
    public int? AniListId { get; set; }
    public int? AniListOffset { get; set; }
    
    public required bool AirReminderEnabled { get; set; }
    public string? AirReminderChannelId { get; set; }
    public string? AirReminderRoleId { get; set; }
    public string? AirReminderUserId { get; set; }
    
    public required bool CongaReminderEnabled { get; set; }
    public TimeSpan? CongaReminderPeriod { get; set; }
    public string? CongaReminderChannelId { get; set; }
    
    public DateTimeOffset? Created { get; set; }
}