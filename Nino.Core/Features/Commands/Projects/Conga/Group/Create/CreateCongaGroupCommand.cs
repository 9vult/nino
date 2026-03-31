// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Conga.Group.Create;

public sealed record CreateCongaGroupCommand(
    ProjectId ProjectId,
    Abbreviation GroupName,
    Abbreviation FirstChild,
    UserId RequestedBy
);
