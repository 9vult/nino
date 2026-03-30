// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Conga.Add;

public sealed record AddCongaEdgeCommand(
    ProjectId ProjectId,
    Alias Current,
    Alias Next,
    UserId RequestedBy
) : ICommand;
