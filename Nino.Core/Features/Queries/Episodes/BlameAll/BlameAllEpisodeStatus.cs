// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.BlameAll;

public sealed record BlameAllEpisodeStatus(
    Number EpisodeNumber,
    DateTimeOffset? AiredAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<BlameAllTaskStatus> Statuses
);
