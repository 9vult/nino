// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.Roster;

public sealed record ProjectRosterStatus(
    Abbreviation Abbreviation,
    List<ProjectRosterRange> Assignees,
    decimal Weight
);
