// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record EpisodeAiredEvent(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    DateTimeOffset AirTime
) : IEvent;
