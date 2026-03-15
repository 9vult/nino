// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Episodes.Add;

public sealed record AddEpisodeCommand(
    ProjectId ProjectId,
    UserId RequestedBy,
    string First,
    int Count = 1,
    string Format = "$"
);
