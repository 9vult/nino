// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities.Conga;

public abstract class CongaNode(Abbreviation name)
{
    private readonly HashSet<CongaNode> _prerequisites = [];
    private readonly HashSet<CongaNode> _dependents = [];

    /// <summary>
    /// Name of the node. Corresponds to a <see cref="Task"/>
    /// </summary>
    public Abbreviation Name { get; } = name;

    /// <summary>
    /// Nodes that must complete to activate this one
    /// </summary>
    public IReadOnlySet<CongaNode> Prerequisites => _prerequisites;

    /// <summary>
    /// Nodes that will be activated when this one is completed
    /// </summary>
    public IReadOnlySet<CongaNode> Dependents => _dependents;

    /// <summary>
    /// If this node is a root node
    /// </summary>
    public bool IsRootNode => Prerequisites.Count == 0;

    /// <summary>
    /// If this node represents a completed task
    /// </summary>
    /// <param name="tasks">List of <see cref="Task"/>s</param>
    /// <returns><see langword="true"/> if this node's corresponding <see cref="Task"/> is complete</returns>
    public abstract bool IsComplete(IList<Task> tasks);

    /// <summary>
    /// Get the nodes activated by the completion of this node
    /// </summary>
    /// <param name="tasks">List of <see cref="Task"/>s</param>
    /// <returns>List of nodes to activate</returns>
    public abstract IReadOnlyList<CongaNode> GetActivatedNodes(IList<Task> tasks);

    /// <summary>
    /// If this node can be activated
    /// </summary>
    /// <param name="tasks">List of <see cref="Task"/>s</param>
    /// <returns><see langword="true"/> if this node can be activated</returns>
    public bool CanBeActivated(IList<Task> tasks)
    {
        return !IsComplete(tasks) && Prerequisites.All(pre => pre.IsComplete(tasks));
    }

    /// <summary>
    /// Add a dependent node
    /// </summary>
    /// <param name="node">Node to add</param>
    internal void AddDependent(CongaNode node) => _dependents.Add(node);

    /// <summary>
    /// Add a prerequisite node
    /// </summary>
    /// <param name="node">Node to add</param>
    internal void AddPrerequisite(CongaNode node) => _prerequisites.Add(node);

    /// <summary>
    /// Remove a dependent node
    /// </summary>
    /// <param name="node">Node to remove</param>
    internal void RemoveDependent(CongaNode node) => _dependents.Remove(node);

    /// <summary>
    /// Remove a prerequisite node
    /// </summary>
    /// <param name="node">Node to remove</param>
    internal void RemovePrerequisite(CongaNode node) => _prerequisites.Remove(node);

    /// <summary>
    /// Get all the descendents of the <paramref name="root"/> node
    /// </summary>
    /// <param name="root">Root node</param>
    /// <returns>List of all descendents of the <paramref name="root"/> node</returns>
    protected static IReadOnlyList<CongaNode> GetSubtree(CongaNode root)
    {
        var result = new List<CongaNode>();
        var queue = new Queue<CongaNode>();
        var visited = new HashSet<CongaNode>();

        queue.Enqueue(root);
        while (queue.TryDequeue(out var node))
        {
            if (!visited.Add(node))
                continue;
            result.Add(node);
            foreach (var dep in node.Dependents)
                queue.Enqueue(dep);
        }
        return result;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CongaNode node && node.Name.Equals(Name);

    /// <inheritdoc />
    public override int GetHashCode() => Name.GetHashCode();

    public static bool operator ==(CongaNode? a, CongaNode? b) => a?.Name == b?.Name;

    public static bool operator !=(CongaNode? a, CongaNode? b) => a?.Name != b?.Name;

    /// <summary>
    /// A node representing a <see cref="Task"/>
    /// </summary>
    /// <param name="name">Task <see cref="Abbreviation"/></param>
    /// <param name="containingGroup">Parent group of the node, if any</param>
    public sealed class TaskNode(Abbreviation name, GroupNode? containingGroup = null)
        : CongaNode(name)
    {
        /// <summary>
        /// Parent group of this node, or <see langword="null"/> if not applicable
        /// </summary>
        public GroupNode? ContainingGroup { get; internal set; } = containingGroup;

        /// <inheritdoc />
        public override bool IsComplete(IList<Task> tasks) =>
            tasks.FirstOrDefault(t => t.Abbreviation == Name)?.IsDone ?? true;

        /// <inheritdoc />
        public override IReadOnlyList<CongaNode> GetActivatedNodes(IList<Task> tasks)
        {
            // If we're a top-level node, we want to activate the dependent nodes
            // If the node is a group, we actually want the root nodes in the group, not the group itself
            if (ContainingGroup is null)
                return Dependents
                    .Where(d => d.CanBeActivated(tasks))
                    .SelectMany(d =>
                        d switch
                        {
                            GroupNode g => g
                                .Children.Where(n => n.IsRootNode && !n.IsComplete(tasks))
                                .ToList(),
                            _ => [d],
                        }
                    )
                    .ToList();

            // We're in a group
            // Our tree is done, so activate the group
            if (GetSubtree(GetRoot()).All(n => n.IsComplete(tasks)))
                return ContainingGroup.GetActivatedNodes(tasks);

            // Our tree isn't done, activate deps
            return Dependents.Where(d => d.CanBeActivated(tasks)).ToList();

            // Group trees are non-merging so there must be one single root ancestor
            CongaNode GetRoot()
            {
                if (IsRootNode)
                    return this;

                var current = this;
                while (true)
                {
                    // Safe cast because groups don't nest
                    var prereqs = current.Prerequisites.Cast<TaskNode>().ToList();
                    if (prereqs.Count == 0)
                        return current;
                    current = prereqs[0];
                }
            }
        }
    }

    /// <summary>
    /// A node representing a group of <see cref="CongaNode.TaskNode"/>s
    /// </summary>
    /// <param name="name">Name of the group</param>
    public sealed class GroupNode(Abbreviation name) : CongaNode(name)
    {
        private readonly List<CongaNode> _children = [];

        /// <summary>
        /// Nodes in the group
        /// </summary>
        public IReadOnlyList<CongaNode> Children => _children;

        /// <summary>
        /// Add a node to the group
        /// </summary>
        /// <param name="node"></param>
        internal void AddChild(CongaNode node)
        {
            if (!_children.Contains(node))
                _children.Add(node);
        }

        /// <summary>
        /// Remove a node from the group
        /// </summary>
        /// <param name="node"></param>
        internal void RemoveChild(CongaNode node)
        {
            _children.Remove(node);
        }

        /// <inheritdoc />
        public override bool IsComplete(IList<Task> tasks)
        {
            return _children.All(n => n.IsComplete(tasks));
        }

        /// <inheritdoc />
        public override IReadOnlyList<CongaNode> GetActivatedNodes(IList<Task> tasks)
        {
            if (!IsComplete(tasks))
                return Children.Where(c => c.IsRootNode && !c.IsComplete(tasks)).ToList();

            // We're complete, head downstream
            return Dependents.Where(d => d.CanBeActivated(tasks)).ToList();
        }
    }
}
