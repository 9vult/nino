// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Episodes.Add;

public sealed record AddEpisodeCommand(
    Guid ProjectId,
    string Format,
    int Quantity,
    Guid RequestedBy
);
