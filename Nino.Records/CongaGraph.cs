using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Nino.Records.Enums;

namespace Nino.Records;

/// <summary>
/// Represents the Conga graph for a project
/// </summary>
public class CongaGraph
{
    /// <summary>
    /// Special nodes for the current position
    /// </summary>
    public static readonly string[] CurrentSpecials = ["$AIR"];

    /// <summary>
    /// Special nodes for the next position
    /// </summary>
    public static readonly string[] NextSpecials = [];

    private readonly Dictionary<string, CongaNode> _nodes = [];

    /// <summary>
    /// All the nodes
    /// </summary>
    [JsonIgnore]
    public ReadOnlyCollection<CongaNode> Nodes => _nodes.Values.ToList().AsReadOnly();

    /// <summary>
    /// Add a new link to the graph
    /// </summary>
    /// <param name="current">Current task abbreviation</param>
    /// <param name="next">Next task abbreviation</param>
    /// <param name="currentType">Type of the current node</param>
    /// <param name="nextType">Type of the next node</param>
    public void Add(
        string current,
        string next,
        CongaNodeType currentType = CongaNodeType.KeyStaff,
        CongaNodeType nextType = CongaNodeType.KeyStaff
    )
    {
        var currentNode = GetOrCreateNode(current, currentType);
        var nextNode = GetOrCreateNode(next, nextType);

        currentNode.Dependents.Add(nextNode);
        nextNode.Prerequisites.Add(currentNode);
    }

    /// <summary>
    /// Remove a link from the graph
    /// </summary>
    /// <param name="current">Current task abbreviation</param>
    /// <param name="next">Next task abbreviation</param>
    public void Remove(string current, string next)
    {
        if (
            !_nodes.TryGetValue(current, out var currentNode)
            || !_nodes.TryGetValue(next, out var nextNode)
        )
            return;

        currentNode.Dependents.Remove(nextNode);
        nextNode.Prerequisites.Remove(currentNode);

        if (currentNode.Dependents.Count == 0 && currentNode.Prerequisites.Count == 0)
            _nodes.Remove(current);
        if (nextNode.Dependents.Count == 0 && nextNode.Prerequisites.Count == 0)
            _nodes.Remove(next);
    }

    /// <summary>
    /// Get prerequisites for a task
    /// </summary>
    /// <param name="abbreviation">Task to look up</param>
    /// <returns>Prerequisite nodes</returns>
    public IEnumerable<CongaNode> GetPrerequisitesFor(string abbreviation)
    {
        return _nodes.TryGetValue(abbreviation, out var node)
            ? node.Prerequisites
            : Enumerable.Empty<CongaNode>();
    }

    /// <summary>
    /// Get dependents of a task
    /// </summary>
    /// <param name="abbreviation">Task to look up</param>
    /// <returns>Dependent nodes</returns>
    public IEnumerable<CongaNode> GetDependentsOf(string abbreviation)
    {
        return _nodes.TryGetValue(abbreviation, out var node)
            ? node.Dependents
            : Enumerable.Empty<CongaNode>();
    }

    /// <summary>
    /// Get if a task is part of the graph
    /// </summary>
    /// <param name="abbreviation">Task to look up</param>
    /// <returns><see langword="true"/> if the task exists in the graph</returns>
    public bool Contains(string abbreviation)
    {
        return _nodes.ContainsKey(abbreviation);
    }

    /// <summary>
    /// Get a node
    /// </summary>
    /// <param name="abbreviation">Task to look up</param>
    /// <returns>The node, or <see langword="null"/></returns>
    public CongaNode? Get(string abbreviation)
    {
        return _nodes.GetValueOrDefault(abbreviation);
    }

    /// <summary>
    /// Gets or creates a node
    /// </summary>
    /// <param name="abbreviation">Task to get or create</param>
    /// <param name="type">Type of conga node</param>
    /// <returns>The node</returns>
    private CongaNode GetOrCreateNode(
        string abbreviation,
        CongaNodeType type = CongaNodeType.KeyStaff
    )
    {
        if (_nodes.TryGetValue(abbreviation, out var node))
            return node;

        node = new CongaNode { Abbreviation = abbreviation, Type = type };
        _nodes[abbreviation] = node;
        return node;
    }

    /// <summary>
    /// Get a list of the edges in the graph
    /// </summary>
    /// <returns>List of edges in the graph</returns>
    public List<CongaEdge> GetEdges()
    {
        var participants = new List<CongaEdge>();
        foreach (var node in Nodes)
        {
            participants.AddRange(
                node.Dependents.Select(dep => new CongaEdge
                {
                    Current = node.Abbreviation,
                    Next = dep.Abbreviation,
                })
            );
        }
        return participants;
    }

    /// <summary>
    /// Serialize the graph into a flat list
    /// </summary>
    /// <returns>Flat list</returns>
    public CongaNodeDto[] Serialize()
    {
        return Nodes
            .Select(n => new CongaNodeDto
            {
                Abbreviation = n.Abbreviation,
                Type = n.Type,
                Dependents = n.Dependents.Select(d => d.Abbreviation).ToArray(),
            })
            .ToArray();
    }

    /// <summary>
    /// Deserialize a flat list to a graph
    /// </summary>
    /// <param name="dtos">Flat list of nodes</param>
    /// <returns>Graph</returns>
    public static CongaGraph Deserialize(CongaNodeDto[] dtos)
    {
        var graph = new CongaGraph();

        // First pass: Create nodes
        foreach (var dto in dtos)
        {
            graph.GetOrCreateNode(dto.Abbreviation, dto.Type);
        }

        // Second pass: Create edges
        foreach (var dto in dtos)
        {
            foreach (var dep in dto.Dependents)
            {
                graph.Add(dto.Abbreviation, dep);
            }
        }

        return graph;
    }
}
