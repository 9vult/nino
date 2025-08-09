
using System.ComponentModel.DataAnnotations;

namespace Nino.Records;

public record Task
{
    [Key]
    public Guid Id { get; set; }
    public required string Abbreviation { get; set; }
    public required bool Done { get; set; }
    public DateTimeOffset? Updated { get; set; }
    public DateTimeOffset? LastReminded { get; set; }
}