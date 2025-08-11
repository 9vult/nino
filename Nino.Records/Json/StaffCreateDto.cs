namespace Nino.Records.Json;

public class StaffCreateDto
{
    public required ulong UserId { get; set; }
    public required Role Role { get; set; }
    public required bool IsPseudo { get; set; } = false;
}
