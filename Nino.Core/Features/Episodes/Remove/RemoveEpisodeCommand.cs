// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Episodes.Remove;

public sealed record RemoveEpisodeCommand(
    Guid ProjectId,
    string FirstEpisodeNumber,
    string LastEpisodeNumber,
    Guid RequestedBy
);
