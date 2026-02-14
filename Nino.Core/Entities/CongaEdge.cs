// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

/// <summary>
/// An edge between <see cref="CongaNode"/>s in a <see cref="CongaGraph"/>
/// </summary>
public class CongaEdge
{
    /// <summary>
    /// Current node abbreviation
    /// </summary>
    public required string Current { get; init; }

    /// <summary>
    /// Next node abbreviation
    /// </summary>
    public required string Next { get; init; }

    public override string ToString()
    {
        return $"{Current} \u2192 {Next}";
    }

    public static CongaEdge FromString(string input)
    {
        var splits = input.Split("\u2192");
        if (splits.Length != 2)
            throw new FormatException();
        return new CongaEdge { Current = splits[0].Trim(), Next = splits[1].Trim() };
    }
}
