namespace Nino.Records.Dtos;

public record RoleDto
{
    public required string Abbreviation { get; set; }
    public required string Name { get; set; }
    public decimal? Weight { get; set; }
}