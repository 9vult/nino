// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.Roster;

public sealed record ProjectRosterRange(
    MappedIdDto<UserId> Assignee,
    List<(Number, Number)> Ranges
);
