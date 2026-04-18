// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Conga.RemoveEdge;

public sealed record RemoveCongaEdgeCommand(
    ProjectId ProjectId,
    Abbreviation From,
    Abbreviation To,
    UserId RequestedBy
) : ICommand;
