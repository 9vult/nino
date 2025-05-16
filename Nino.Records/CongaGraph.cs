using System.Collections.ObjectModel;

namespace Nino.Records;

/// <summary>
/// Represents the Conga graph for a project
/// </summary>
public class CongaGraph
{
    private readonly Dictionary<string, CongaNode> _nodes = [];

    /// <summary>
    /// All the nodes
    /// </summary>
    public ReadOnlyCollection<CongaNode> Nodes => _nodes.Values.ToList().AsReadOnly();

    /// <summary>
    /// Add a new link to the graph
    /// </summary>
    /// <param name="current">Current task abbreviation</param>
    /// <param name="next">Next task abbreviation</param>
    public void Add(string current, string next)
    {
        var currentNode = GetOrCreateNode(current);
        var nextNode = GetOrCreateNode(next);
        
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
        if (!_nodes.TryGetValue(current, out var currentNode) || !_nodes.TryGetValue(next, out var nextNode)) return;
        
        currentNode.Dependents.Remove(nextNode);
        nextNode.Prerequisites.Remove(currentNode);
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
    /// <returns>The node</returns>
    private CongaNode GetOrCreateNode(string abbreviation)
    {
        if (_nodes.TryGetValue(abbreviation, out var node)) return node;
        
        node = new CongaNode { Abbreviation = abbreviation };
        _nodes[abbreviation] = node;
        return node;
    }

    /// <summary>
    /// Serialize the graph into a flat list
    /// </summary>
    /// <returns>Flat list</returns>
    public List<CongaNodeDto> Serialize()
    {
        var participants = new List<CongaNodeDto>();
        foreach (var node in Nodes)
        {
            participants.AddRange(node.Dependents.Select(dep => 
                new CongaNodeDto { Current = node.Abbreviation, Next = dep.Abbreviation }));
        }
        return participants;
    }

    /// <summary>
    /// Deserialize a flat list to a graph
    /// </summary>
    /// <param name="nodes">Flat list of nodes</param>
    /// <returns>Graph</returns>
    public static CongaGraph Deserialize(CongaNodeDto[] nodes)
    {
        var graph = new CongaGraph();
        foreach (var participant in nodes)
            graph.Add(participant.Current, participant.Next);
        return graph;
    }
}