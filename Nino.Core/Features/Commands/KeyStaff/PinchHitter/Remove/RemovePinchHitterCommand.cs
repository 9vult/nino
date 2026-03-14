// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.KeyStaff.PinchHitter.Remove;

public sealed record RemovePinchHitterCommand(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    TaskId TaskId,
    UserId RequestedBy
);
