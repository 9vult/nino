// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Conga.Remove;

public sealed record RemoveCongaEdgeCommand(
    ProjectId ProjectId,
    Abbreviation Current,
    Abbreviation Next,
    UserId RequestedBy
) : ICommand;
