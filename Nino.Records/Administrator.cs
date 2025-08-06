using System.ComponentModel.DataAnnotations;

namespace Nino.Records;

public class Administrator
{
    [Key]
    public Guid Id { get; set; }
    public ulong UserId { get; set; }
}