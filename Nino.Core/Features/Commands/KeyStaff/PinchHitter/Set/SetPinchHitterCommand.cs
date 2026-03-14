// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.KeyStaff.PinchHitter.Set;

public sealed record SetPinchHitterCommand(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    TaskId TaskId,
    UserId MemberId,
    UserId RequestedBy
);
