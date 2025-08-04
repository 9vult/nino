using Nino.Records.Dtos;

namespace Nino.Records.Mappers;

public static class CongaGraphMapper
{
    public static List<CongaNodeDto> ToDto(this CongaGraph domain)
    {
        return domain.Nodes.Select(n => new CongaNodeDto
        {
            Abbreviation = n.Abbreviation,
            Type = n.Type,
            Dependents = n.Dependents.Select(d => d.Abbreviation).ToList()
        }).ToList();
    }

    public static CongaGraph FromDto(this List<CongaNodeDto> dtos)
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