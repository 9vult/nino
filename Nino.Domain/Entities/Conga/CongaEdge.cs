// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities.Conga;

/// <summary>
/// An edge between <see cref="CongaNode"/>s in a <see cref="CongaGraph"/>
/// </summary>
public class CongaEdge
{
    /// <summary>
    /// Current node abbreviation
    /// </summary>
    public required Abbreviation Current { get; init; }

    /// <summary>
    /// Next node abbreviation
    /// </summary>
    public required Abbreviation Next { get; init; }

    public override string ToString()
    {
        return $"{Current} \u2192 {Next}";
    }

    public static CongaEdge FromString(string input)
    {
        var splits = input.Split("\u2192");
        if (splits.Length != 2)
            throw new FormatException();
        return new CongaEdge
        {
            Current = Abbreviation.From(splits[0].Trim()),
            Next = Abbreviation.From(splits[1].Trim()),
        };
    }
}
