// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.Blame;

public sealed record BlameQuery(
    ProjectId ProjectId,
    EpisodeId? EpisodeId,
    bool IncludePseudo,
    UserId RequestedBy
) : IQuery;
