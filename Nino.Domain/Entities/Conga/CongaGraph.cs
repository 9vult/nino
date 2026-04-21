// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Dtos;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities.Conga;

public sealed class CongaGraph
{
    private readonly Dictionary<Abbreviation, CongaNode> _nodes = [];
    private readonly List<CongaNode> _children = [];

    public IReadOnlyList<CongaNode> Nodes => _nodes.Values.ToList();
    public IReadOnlyList<CongaNode> Children => _children;

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

                        // Check for illegal tree if no group nodes involved
                        if (
                            fromNode is not CongaNode.GroupNode
                            && toNode is not CongaNode.GroupNode
                        )
                        {
                            var fromRoot = fromNode;
                            var toRoot = toNode;
                            while (!fromRoot.IsRootNode)
                                fromRoot = fromRoot.Prerequisites.First();
                            while (!toRoot.IsRootNode)
                                toRoot = toRoot.Prerequisites.First();
                            if (fromRoot != toRoot)
                                return CongaModificationResult.IllegalTree;
                        }

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
                fromNode ??= from.Value.StartsWith('@')
                    ? new CongaNode.GroupNode(from)
                    : new CongaNode.TaskNode(from);
                toNode ??= to.Value.StartsWith('@')
                    ? new CongaNode.GroupNode(to)
                    : new CongaNode.TaskNode(to);

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
                fromNode ??= from.Value.StartsWith('@')
                    ? new CongaNode.GroupNode(from)
                    : new CongaNode.TaskNode(from);
                toNode ??= to.Value.StartsWith('@')
                    ? new CongaNode.GroupNode(to)
                    : new CongaNode.TaskNode(to);

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

    public CongaModificationResult RemoveGroupMember(Abbreviation groupName, Abbreviation name)
    {
        if (
            !_nodes.TryGetValue(groupName, out var groupNode)
            || groupNode is not CongaNode.GroupNode group
        )
            return CongaModificationResult.NoGroup;
        if (!_nodes.TryGetValue(name, out var node))
            return CongaModificationResult.NotFound;

        group.RemoveChild(node);
        UnregisterNode(node);

        foreach (var dep in CongaNode.GetSubtree(node))
        {
            group.RemoveChild(dep);
            UnregisterNode(dep);
        }
        return CongaModificationResult.Success;
    }

    public static CongaGraph FromDto(CongaGraphDto dto)
    {
        var graph = new CongaGraph();

        foreach (var groupDto in dto.Groups)
        {
            var group = new CongaNode.GroupNode(groupDto.Name);
            graph.AddDirectChild(group);
            foreach (var edge in groupDto.Edges)
            {
                // Root child node with no deps
                if (edge.From == group.Name)
                {
                    var child = new CongaNode.TaskNode(edge.To);
                    group.AddChild(child);
                    graph.RegisterNode(child);
                    continue;
                }

                var from = group.Children.FirstOrDefault(x => x.Name == edge.From);
                var to = group.Children.FirstOrDefault(x => x.Name == edge.To);

                if (from is null)
                {
                    from = new CongaNode.TaskNode(edge.From);
                    group.AddChild(from);
                    graph.RegisterNode(from);
                }

                if (to is null)
                {
                    to = new CongaNode.TaskNode(edge.To);
                    group.AddChild(to);
                    graph.RegisterNode(to);
                }

                from.AddDependent(to);
                to.AddPrerequisite(from);
            }
        }

        foreach (var edge in dto.Edges)
        {
            var from = graph.Children.FirstOrDefault(x => x.Name == edge.From);
            var to = graph.Children.FirstOrDefault(x => x.Name == edge.To);

            if (from is null)
            {
                from = new CongaNode.TaskNode(edge.From);
                graph.AddDirectChild(from);
            }

            if (to is null)
            {
                to = new CongaNode.TaskNode(edge.To);
                graph.AddDirectChild(to);
            }

            from.AddDependent(to);
            to.AddPrerequisite(from);
        }

        return graph;
    }

    public CongaGraphDto ToDto()
    {
        var dto = new CongaGraphDto { Edges = [], Groups = [] };

        foreach (var group in Nodes.OfType<CongaNode.GroupNode>())
        {
            var groupDto = new CongaNodeDto.GroupNodeDto { Name = group.Name, Edges = [] };
            foreach (var child in group.Children)
            {
                // Root child node with no deps
                if (child is { IsRootNode: true, Dependents.Count: 0 })
                {
                    groupDto.Edges.Add(new CongaEdgeDto { From = group.Name, To = child.Name });
                    continue;
                }

                foreach (var dep in child.Dependents)
                    groupDto.Edges.Add(new CongaEdgeDto { From = child.Name, To = dep.Name });
            }
            dto.Groups.Add(groupDto);
        }

        foreach (var child in Children)
        {
            foreach (var dep in child.Dependents)
                dto.Edges.Add(new CongaEdgeDto { From = child.Name, To = dep.Name });
        }
        return dto;
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
