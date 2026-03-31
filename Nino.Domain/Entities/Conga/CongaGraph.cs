// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities.Conga;

public sealed class CongaGraph
{
    private readonly Dictionary<Abbreviation, CongaNode> _nodes = [];
    private readonly List<CongaNode> _children = [];

    public IReadOnlyList<CongaNode> Nodes => _nodes.Values.ToList();
    public IReadOnlyList<CongaNode> Children => _children;

    // TODO: Create/remove groups, add/remove from groups
    // TODO: Activation evaluations
    // Note that Groups need to differentiate between root and non-root nodes for activations

    public CongaModificationResult AddEdge(Abbreviation from, Abbreviation to)
    {
        switch (_nodes.TryGetValue(from, out var fromNode), _nodes.TryGetValue(to, out var toNode))
        {
            case (true, true):
                if (fromNode is null || toNode is null)
                    throw new NullReferenceException();
                if (fromNode == toNode)
                    return CongaModificationResult.SelfLoop;

                var fromGroup = GetContainingGroup(fromNode);
                var toGroup = GetContainingGroup(toNode);

                switch (fromGroup is null, toGroup is null)
                {
                    case (true, true): // both outer
                    case (false, false) when fromGroup == toGroup: // both in same group
                        fromNode.AddDependent(toNode);
                        toNode.AddPrerequisite(fromNode);
                        return CongaModificationResult.Success;

                    case (false, false): // different groups
                    case (true, false): // one outer, one internal
                    case (false, true):
                        return CongaModificationResult.MixedGroups;
                }
            case (true, false) or (false, true): // One of the nodes exists, the other doesn't
                fromNode ??= new CongaNode.TaskNode(from);
                toNode ??= new CongaNode.TaskNode(to);

                fromNode.AddDependent(toNode);
                toNode.AddPrerequisite(fromNode);

                var group = GetContainingGroup(fromNode) ?? GetContainingGroup(toNode);
                if (group is null)
                {
                    AddDirectChild(fromNode);
                    AddDirectChild(toNode);
                    return CongaModificationResult.Success;
                }
                group.AddChild(fromNode);
                group.AddChild(toNode);
                RegisterNode(fromNode);
                RegisterNode(toNode);
                return CongaModificationResult.Success;
            case (false, false): // Neither node exists
                fromNode ??= new CongaNode.TaskNode(from);
                toNode ??= new CongaNode.TaskNode(to);

                fromNode.AddDependent(toNode);
                toNode.AddPrerequisite(fromNode);

                AddDirectChild(fromNode);
                AddDirectChild(toNode);
                return CongaModificationResult.Success;
        }
    }

    private void AddDirectChild(CongaNode node)
    {
        if (!_children.Contains(node))
            _children.Add(node);
        _nodes.TryAdd(node.Name, node);
    }

    private void RegisterNode(CongaNode node)
    {
        _nodes.TryAdd(node.Name, node);
    }

    private void RemoveDirectChild(CongaNode node)
    {
        _children.Remove(node);
        _nodes.Remove(node.Name);
    }

    private void UnregisterNode(CongaNode node)
    {
        _nodes.Remove(node.Name);
    }

    private CongaNode.GroupNode? GetContainingGroup(CongaNode node)
    {
        return _nodes
            .Values.OfType<CongaNode.GroupNode>()
            .FirstOrDefault(g => g.Children.Any(m => m.Name == node.Name));
    }
}
