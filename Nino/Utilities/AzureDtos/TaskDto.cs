namespace Nino.Utilities.AzureDtos;

public record TaskDto
{
    public required string Abbreviation { get; set; }
    public required bool Done { get; set; }
    public DateTimeOffset? Updated { get; set; }
    public DateTimeOffset? LastReminded { get; set; }
}