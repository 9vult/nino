namespace Nino.Utilities.AzureDtos;

public record EpisodeDto
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public required string GuildId { get; set; }
    
    public required string Number { get; set; }
    public required bool Done { get; set; }
    public required bool ReminderPosted { get; set; }
    
    public required List<StaffDto> AdditionalStaff { get; set; }
    public required List<PinchHitterDto> PinchHitters { get; set; }
    public required List<TaskDto> Tasks { get; set; }
    public DateTimeOffset? Updated { get; set; }
}