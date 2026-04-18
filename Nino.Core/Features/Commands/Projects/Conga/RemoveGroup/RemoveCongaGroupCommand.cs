// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Conga.RemoveGroup;

public sealed record RemoveCongaGroupCommand(
    ProjectId ProjectId,
    Abbreviation Name,
    UserId RequestedBy
) : ICommand;
