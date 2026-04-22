// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.Roster;

public sealed record ProjectRosterQuery(ProjectId ProjectId, UserId RequestedBy) : IQuery;
