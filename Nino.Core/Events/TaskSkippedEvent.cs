// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Events.Episode;

public record TaskSkippedEvent(
    Guid ProjectId,
    Guid EpisodeId,
    string Abbreviation,
    DateTimeOffset Timestamp
) : IEvent;
