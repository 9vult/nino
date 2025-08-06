using System.ComponentModel.DataAnnotations;

namespace Nino.Records;

public class Episode
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public required ulong GuildId { get; set; }
    [MaxLength(32)]
    public required string Number { get; set; }
    public required bool Done { get; set; }
    public required bool ReminderPosted { get; set; }
    public DateTime? Updated { get; set; }

    public List<Task> Tasks { get; set; } = [];
    public List<Staff> AdditionalStaff { get; set; } = [];
    public List<PinchHitter> PinchHitters { get; set; } = [];
    public Project Project { get; set; } = null!;

    public override string ToString ()
    {
        return $"E[{Id} ({Number})]";
    }
}