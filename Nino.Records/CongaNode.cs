using Nino.Records.Enums;

namespace Nino.Records;

/// <summary>
/// A node in a conga graph
/// </summary>
public class CongaNode
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
    
    /// <summary>
    /// Type of node. Defaults to <see cref="CongaNodeType.KeyStaff"/>.
    /// </summary>
    public required CongaNodeType Type { get; set; } = CongaNodeType.KeyStaff;
}

/// <summary>
/// DTO for serializing the graph
/// </summary>
public class CongaNodeDto
{
    public required string Abbreviation { get; init; }
    public required CongaNodeType Type { get; init; } = CongaNodeType.KeyStaff;
    
    public required string[] Dependents { get; init; } = [];
}

/// <summary>
/// Edge in the Conga graph
/// </summary>
public class CongaEdge
{
    public required string Current { get; init; }
    public required string Next { get; init; }

    public override string ToString()
    {
        return $"{Current} \u2192 {Next}";
    }

    public static CongaEdge FromString(string input)
    {
        var splits = input.Split("\u2192");
        if (splits.Length != 2) throw new FormatException();
        return new CongaEdge
        {
            Current = splits[0].Trim(),
            Next = splits[1].Trim(),
        };
    }
}
