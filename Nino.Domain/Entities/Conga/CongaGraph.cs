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
        if (from == to)
            return CongaModificationResult.SelfLoop;

        switch (_nodes.TryGetValue(from, out var fromNode), _nodes.TryGetValue(to, out var toNode))
        {
            case (true, true):
                if (fromNode is null || toNode is null)
                    throw new NullReferenceException();
                if (DetectCycle(toNode, fromNode))
                    return CongaModificationResult.Cycle;

                var fromGroup = GetContainingGroup(fromNode);
                var toGroup = GetContainingGroup(toNode);

                switch (fromGroup is null, toGroup is null)
                {
                    case (true, true): // both outer
                    case (false, false) when fromGroup == toGroup: // both in same group
                        if (fromNode.Dependents.Contains(toNode))
                            return CongaModificationResult.Duplicate;

                        // Check for illegal tree
                        var fromRoot = fromNode;
                        var toRoot = toNode;
                        while (!fromRoot.IsRootNode)
                            fromRoot = fromRoot.Prerequisites.First();
                        while (!toRoot.IsRootNode)
                            toRoot = toRoot.Prerequisites.First();
                        if (fromRoot != toRoot)
                            return CongaModificationResult.IllegalTree;

                        // We're all set!
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

                // Configure the group relation
                if (fromNode is CongaNode.TaskNode fromTaskNode)
                    fromTaskNode.ContainingGroup = group;
                if (toNode is CongaNode.TaskNode toTaskNode)
                    toTaskNode.ContainingGroup = group;

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

    public CongaModificationResult AddGroup(Abbreviation name)
    {
        if (_nodes.TryGetValue(name, out _))
            return CongaModificationResult.Duplicate;
        var group = new CongaNode.GroupNode(name);
        AddDirectChild(group);
        return CongaModificationResult.Success;
    }

    public CongaModificationResult AddGroupMember(Abbreviation groupName, Abbreviation name)
    {
        if (
            !_nodes.TryGetValue(groupName, out var groupNode)
            || groupNode is not CongaNode.GroupNode group
        )
            return CongaModificationResult.NoGroup;
        if (_nodes.TryGetValue(name, out _))
            return CongaModificationResult.Duplicate;

        var newNode = new CongaNode.TaskNode(name, group);
        group.AddChild(newNode);
        RegisterNode(newNode);
        return CongaModificationResult.Success;
    }

    public CongaModificationResult RemoveEdge(Abbreviation from, Abbreviation to)
    {
        if (_nodes.TryGetValue(from, out var fromNode) && _nodes.TryGetValue(to, out var toNode))
        {
            if (!fromNode.Dependents.Contains(toNode))
                return CongaModificationResult.NoLink;

            fromNode.RemoveDependent(toNode);
            toNode.RemovePrerequisite(fromNode);

            if (fromNode.Dependents.Count == 0 && fromNode.Prerequisites.Count == 0)
            {
                switch (fromNode)
                {
                    case CongaNode.TaskNode { ContainingGroup: not null } taskNode:
                        taskNode.ContainingGroup.RemoveChild(fromNode);
                        UnregisterNode(fromNode);
                        break;
                    case CongaNode.TaskNode:
                        RemoveDirectChild(fromNode);
                        break;
                    case CongaNode.GroupNode:
                        break;
                }
            }
            if (toNode.Dependents.Count == 0 && toNode.Prerequisites.Count == 0)
            {
                switch (toNode)
                {
                    case CongaNode.TaskNode { ContainingGroup: not null } taskNode:
                        taskNode.ContainingGroup.RemoveChild(toNode);
                        UnregisterNode(toNode);
                        break;
                    case CongaNode.TaskNode:
                        RemoveDirectChild(toNode);
                        break;
                    case CongaNode.GroupNode:
                        break;
                }
            }
            return CongaModificationResult.Success;
        }
        return CongaModificationResult.NotFound;
    }

    public CongaModificationResult RemoveGroup(Abbreviation name)
    {
        if (
            !_nodes.TryGetValue(name, out var groupNode)
            || groupNode is not CongaNode.GroupNode group
        )
            return CongaModificationResult.NoGroup;

        foreach (var pre in group.Prerequisites)
        {
            pre.RemoveDependent(groupNode);
            if (pre.Dependents.Count == 0 && pre.Prerequisites.Count == 0)
            {
                switch (pre)
                {
                    case CongaNode.TaskNode { ContainingGroup: not null } taskNode:
                        taskNode.ContainingGroup.RemoveChild(pre);
                        UnregisterNode(pre);
                        break;
                    case CongaNode.TaskNode:
                        RemoveDirectChild(pre);
                        break;
                    case CongaNode.GroupNode:
                        break;
                }
            }
        }
        foreach (var dep in group.Dependents)
        {
            dep.RemovePrerequisite(groupNode);
            if (dep.Dependents.Count == 0 && dep.Prerequisites.Count == 0)
            {
                switch (dep)
                {
                    case CongaNode.TaskNode { ContainingGroup: not null } taskNode:
                        taskNode.ContainingGroup.RemoveChild(dep);
                        UnregisterNode(dep);
                        break;
                    case CongaNode.TaskNode:
                        RemoveDirectChild(dep);
                        break;
                    case CongaNode.GroupNode:
                        break;
                }
            }
        }

        foreach (var child in group.Children)
            UnregisterNode(child);

        RemoveDirectChild(groupNode);
        return CongaModificationResult.Success;
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

    private static bool DetectCycle(CongaNode toNode, CongaNode fromNode)
    {
        var visited = new HashSet<CongaNode>();
        var stack = new Stack<CongaNode>();
        stack.Push(toNode);

        while (stack.TryPop(out var current))
        {
            if (!visited.Add(current))
                continue;
            foreach (var dep in current.Dependents)
            {
                if (dep == fromNode)
                    return true; // found a path back to the new prerequisite
                stack.Push(dep);
            }
        }
        return false;
    }

    private CongaNode.GroupNode? GetContainingGroup(CongaNode node)
    {
        return _nodes
            .Values.OfType<CongaNode.GroupNode>()
            .FirstOrDefault(g => g.Children.Any(m => m.Name == node.Name));
    }
}
