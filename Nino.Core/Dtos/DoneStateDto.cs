// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos;

public sealed record DoneStateDto(
    Guid ProjectId,
    string WorkingEpisodeNumber,
    string TaskEpisodeNumber,
    string Abbreviation,
    string TaskName,
    Guid RequestedBy
);
