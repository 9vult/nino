// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Dtos;

public class CongaGraphDto
{
    public required List<CongaNodeDto.GroupNodeDto> Groups { get; set; }
    public required List<CongaEdgeDto> Edges { get; set; }

    public static CongaGraphDto Empty => new() { Groups = [], Edges = [] };
}
