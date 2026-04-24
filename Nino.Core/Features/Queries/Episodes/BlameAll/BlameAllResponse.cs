// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Queries.Episodes.BlameAll;

public sealed record BlameAllResponse(
    IReadOnlyList<BlameAllEpisodeStatus> Episodes,
    string Motd,
    int Page,
    int PageCount
);
