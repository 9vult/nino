// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.KeyStaff.PinchHitter.Set;

public sealed record SetPinchHitterCommand(
    Guid ProjectId,
    string EpisodeNumber,
    string Abbreviation,
    Guid UserId,
    Guid RequestedBy
);
