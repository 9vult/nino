// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.Roster;

public sealed record EpisodeRosterQuery(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    UserId RequestedBy
) : IQuery;
