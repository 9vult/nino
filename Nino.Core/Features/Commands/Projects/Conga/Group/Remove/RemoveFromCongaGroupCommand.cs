// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Conga.Group.Remove;

public sealed record RemoveFromCongaGroupCommand(
    ProjectId ProjectId,
    Alias GroupName,
    Alias Alias,
    UserId RequestedBy
);
