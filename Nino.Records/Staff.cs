namespace Nino.Records;

public class Staff
{
    public required ulong UserId { get; set; }
    public required Role Role { get; set; }
    public required bool IsPseudo { get; set; }
}