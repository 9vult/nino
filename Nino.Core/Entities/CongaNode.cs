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

    /// <summary>
    /// Check if the node is complete
    /// </summary>
    /// <param name="episode">Episode to check for</param>
    /// <remarks>If a task is not applicable, it is treated as complete.</remarks>
    /// <returns><see langword="true"/> if complete</returns>
    private bool IsComplete(Episode episode)
    {
        switch (Type)
        {
            case CongaNodeType.KeyStaff:
            case CongaNodeType.AdditionalStaff:
                return episode.Tasks.FirstOrDefault(t => t.Abbreviation == Abbreviation)?.IsDone
                    ?? true;
            case CongaNodeType.Group:
                var members = Dependents.Where(d => d.Dependents.Contains(this));
                return members.All(t => t.IsComplete(episode));
            default:
                return true;
        }
    }

    /// <summary>
    /// Check if the node can be activated
    /// </summary>
    /// <param name="episode">Episode to check for</param>
    /// <remarks>If a task is not applicable, it is treated as unactivatable.</remarks>
    /// <returns><see langword="true"/> if activatable</returns>
    private bool CanActivate(Episode episode)
    {
        switch (Type)
        {
            case CongaNodeType.KeyStaff:
            case CongaNodeType.AdditionalStaff:
                return Prerequisites.All(p => p.IsComplete(episode));
            case CongaNodeType.Group:
                var members = Dependents.Where(d => d.Dependents.Contains(this));
                var upstream = Prerequisites.Where(p => !members.Contains(p));
                return upstream.All(t => t.IsComplete(episode));
            default:
                return false;
        }
    }

    /// <summary>
    /// Get the nodes that can be activated
    /// </summary>
    /// <param name="episode">Episode to check for</param>
    /// <returns>List of nodes that can be activated</returns>
    public List<CongaNode> GetActivatedNodes(Episode episode)
    {
        var result = new List<CongaNode>();
        foreach (var dependent in Dependents)
        {
            if (!dependent.CanActivate(episode))
                continue;

            switch (dependent.Type)
            {
                case CongaNodeType.KeyStaff:
                case CongaNodeType.AdditionalStaff:
                    if (!dependent.IsComplete(episode))
                        result.Add(dependent);
                    break;
                case CongaNodeType.Group:
                    // If group members aren't done, only ping group members
                    var members = dependent
                        .Dependents.Where(d => d.Dependents.Contains(dependent))
                        .ToList();
                    var incompleteMembers = members
                        .Where(member => !member.IsComplete(episode))
                        .ToList();
                    if (incompleteMembers.Count != 0)
                    {
                        result.AddRange(incompleteMembers);
                        break;
                    }
                    // Otherwise, only ping non-members
                    result.AddRange(
                        dependent.Dependents.Where(d =>
                            !members.Contains(d) && !d.IsComplete(episode) && d.CanActivate(episode)
                        )
                    );
                    break;
                default:
                    continue;
            }
        }
        return result;
    }
}
