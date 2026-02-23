// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Events;

public record TaskCompletedEvent(
    Guid ProjectId,
    Guid EpisodeId,
    string Abbreviation,
    bool WasSkipped,
    DateTimeOffset Timestamp
) : IEvent;
