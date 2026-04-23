// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.ValidateRelease;

public sealed record ValidateReleaseQuery(
    ProjectId ProjectId,
    Number FirstEpisode,
    Number LastEpisode
) : IQuery;
