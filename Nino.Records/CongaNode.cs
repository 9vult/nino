namespace Nino.Records;

/// <summary>
/// A node in a conga graph
/// </summary>
public record CongaNode
{
    /// <summary>
    /// Abbreviation of the task represented by the node
    /// </summary>
    public required string Abbreviation { get; init; }
    
    /// <summary>
    /// List of nodes depending on this node
    /// </summary>
    public HashSet<CongaNode> Dependents { get; init; } = [];
    /// <summary>
    /// List of nodes this node depends on
    /// </summary>
    public HashSet<CongaNode> Prerequisites { get; init; } = [];
}

/// <summary>
/// DTO for serializing the graph
/// </summary>
public record CongaNodeDto
{
    public required string Current { get; init; }
    public required string Next { get; init; }
}
