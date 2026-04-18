// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Conga.AddEdge;

public sealed record AddCongaEdgeCommand(
    ProjectId ProjectId,
    Abbreviation From,
    Abbreviation To,
    UserId RequestedBy
) : ICommand;
