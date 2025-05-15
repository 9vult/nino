using System.Collections.ObjectModel;

namespace Nino.Records;

public class CongaGraph
{
    private readonly Dictionary<string, CongaNode> _nodes = [];

    public ReadOnlyCollection<CongaNode> Nodes => _nodes.Values.ToList().AsReadOnly();

    public void Add(string current, string next)
    {
        var currentNode = GetOrCreateNode(current);
        var nextNode = GetOrCreateNode(next);
        
        currentNode.Dependents.Add(nextNode);
        nextNode.Prerequisites.Add(currentNode);
    }

    public void Remove(string current, string next)
    {
        if (!_nodes.TryGetValue(current, out var currentNode) || !_nodes.TryGetValue(next, out var nextNode)) return;
        
        currentNode.Dependents.Remove(nextNode);
        nextNode.Prerequisites.Remove(currentNode);
    }

    public IEnumerable<CongaNode> GetPrerequisitesFor(string abbreviation)
    {
        return _nodes.TryGetValue(abbreviation, out var node) 
            ? node.Prerequisites 
            : Enumerable.Empty<CongaNode>();
    }

    public IEnumerable<CongaNode> GetDependentsOf(string abbreviation)
    {
        return _nodes.TryGetValue(abbreviation, out var node)
            ? node.Dependents
            : Enumerable.Empty<CongaNode>();
    }
    
    public bool Contains(string abbreviation)
    {
        return _nodes.ContainsKey(abbreviation);
    }

    public CongaNode? Get(string abbreviation)
    {
        return _nodes.GetValueOrDefault(abbreviation);
    }

    private CongaNode GetOrCreateNode(string abbreviation)
    {
        if (_nodes.TryGetValue(abbreviation, out var node)) return node;
        
        node = new CongaNode { Abbreviation = abbreviation };
        _nodes[abbreviation] = node;
        return node;
    }

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

    public static CongaGraph Deserialize(List<CongaNodeDto> nodes)
    {
        var graph = new CongaGraph();
        foreach (var participant in nodes)
            graph.Add(participant.Current, participant.Next);
        return graph;
    }
}