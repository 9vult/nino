
using System.ComponentModel.DataAnnotations;

namespace Nino.Records;

public class Alias
{
    [Key]
    public Guid Id { get; set; }
    public required string Value { get; set; }
}