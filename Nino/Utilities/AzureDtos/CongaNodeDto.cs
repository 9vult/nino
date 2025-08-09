using Nino.Records.Enums;

namespace Nino.Utilities.AzureDtos;

public class CongaNodeDto
{
    public required string Abbreviation { get; init; }
    public required CongaNodeType Type { get; init; } = CongaNodeType.KeyStaff;
    
    public required List<string> Dependents { get; init; } = [];
}