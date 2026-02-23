// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Done;

public sealed record DoneCommand(
    Guid ProjectId,
    string EpisodeNumber,
    string Abbreviation,
    Guid RequestedBy
);
