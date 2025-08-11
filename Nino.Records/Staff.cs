using System.ComponentModel.DataAnnotations;

namespace Nino.Records;

public class Staff
{
    [Key]
    public Guid Id { get; set; }
    public required ulong UserId { get; set; }
    public required Role Role { get; set; }
    public required bool IsPseudo { get; set; } = false;
}
