// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Entities.Conga;

public sealed class CongaEdge(CongaNode from, CongaNode to)
{
    public CongaNode From { get; } = from;
    public CongaNode To { get; } = to;
}
