using System.ComponentModel.DataAnnotations;

namespace Nino.Records;

public class PinchHitter
{
    [Key]
    public required Guid Id { get; set; }
    public required ulong UserId { get; set; }
    public required string Abbreviation { get; set; }
}