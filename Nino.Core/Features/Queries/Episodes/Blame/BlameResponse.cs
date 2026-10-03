// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.Blame;

public sealed record BlameResponse(
    Number EpisodeNumber,
    AniListId AniListId,
    int AniListOffset,
    bool IsAirTimeEstimated,
    DateTimeOffset? AiredAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<BlameStatus> Statuses,
    bool IsSingleEpisodeMovie,
    string Motd
);
