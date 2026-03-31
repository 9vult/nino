// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities.Conga;

public abstract class CongaNode(Abbreviation name)
{
    public Abbreviation Name { get; } = name;

    private readonly HashSet<CongaNode> _prerequisites = [];
    private readonly HashSet<CongaNode> _dependents = [];

    public IReadOnlySet<CongaNode> Prerequisites => _prerequisites;
    public IReadOnlySet<CongaNode> Dependents => _dependents;

    public bool IsRootNode => Prerequisites.Count == 0;

    internal void AddDependent(CongaNode node) => _dependents.Add(node);

    internal void AddPrerequisite(CongaNode node) => _prerequisites.Add(node);

    public abstract bool IsComplete(IList<Task> tasks);

    public sealed class TaskNode(Abbreviation name) : CongaNode(name)
    {
        /// <inheritdoc />
        public override bool IsComplete(IList<Task> tasks) =>
            tasks.FirstOrDefault(t => t.Abbreviation == Name)?.IsDone ?? true;
    }

    public sealed class GroupNode(Abbreviation name) : CongaNode(name)
    {
        private readonly List<CongaNode> _children = [];

        public IReadOnlyList<CongaNode> Children => _children;

        internal void AddChild(CongaNode node)
        {
            if (!_children.Contains(node))
                _children.Add(node);
        }

        internal void RemoveChild(CongaNode node)
        {
            _children.Remove(node);
        }

        /// <inheritdoc />
        public override bool IsComplete(IList<Task> tasks)
        {
            return _children.All(n => n.IsComplete(tasks));
        }
    }
}
