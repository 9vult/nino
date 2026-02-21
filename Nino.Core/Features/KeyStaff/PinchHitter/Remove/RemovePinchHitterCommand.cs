// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.KeyStaff.PinchHitter.Remove;

public sealed record RemovePinchHitterCommand(
    Guid ProjectId,
    string EpisodeNumber,
    string Abbreviation,
    Guid RequestedBy
);
