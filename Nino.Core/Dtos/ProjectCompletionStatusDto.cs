// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos;

public sealed record ProjectCompletionStatusDto(
    int CompletedEpisodeCount,
    int IncompleteEpisodeCount
);
