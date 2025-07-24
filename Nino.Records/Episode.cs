namespace Nino.Records;

public class Episode
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public required ulong GuildId { get; set; }
    public required string Number { get; set; }
    public required bool Done { get; set; }
    public required bool ReminderPosted { get; set; }
    public required Staff[] AdditionalStaff { get; set; }
    public required PinchHitter[] PinchHitters { get; set; }
    public required Task[] Tasks { get; set; }
    public DateTimeOffset? Updated { get; set; }

    public override string ToString()
    {
        return $"E[{Id} ({Number})]";
    }
}