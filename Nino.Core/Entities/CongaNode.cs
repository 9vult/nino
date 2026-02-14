// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Entities;

/// <summary>
/// A node in a <see cref="CongaGraph"/>
/// </summary>
public class CongaNode
{
    /// <summary>
    /// Abbreviation of the task represented by the node
    /// </summary>
    public required string Abbreviation { get; set; }

    /// <summary>
    /// Type of node. Defaults to <see cref="CongaNodeType.KeyStaff"/>.
    /// </summary>
    public required CongaNodeType Type { get; set; } = CongaNodeType.KeyStaff;

    /// <summary>
    /// List of nodes depending on this node
    /// </summary>
    public HashSet<CongaNode> Dependents { get; set; } = [];

    /// <summary>
    /// List of nodes this node depends on
    /// </summary>
    public HashSet<CongaNode> Prerequisites { get; set; } = [];
}
