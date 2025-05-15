namespace Nino.Records;

public record CongaNode
{
    public required string Abbreviation { get; init; }
    
    public HashSet<CongaNode> Dependents { get; init; } = [];
    public HashSet<CongaNode> Prerequisites { get; init; } = [];
}

public record CongaNodeDto
{
    public required string Current { get; init; }
    public required string Next { get; init; }
}
