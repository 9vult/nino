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

    public override string ToString()
    {
        return $"{Current} \u2192 {Next}";
    }

    public static CongaNodeDto FromString(string input)
    {
        var splits = input.Split("\u2192");
        if (splits.Length != 2) throw new FormatException();
        return new CongaNodeDto
        {
            Current = splits[0].Trim(),
            Next = splits[1].Trim(),
        };
    }
}
