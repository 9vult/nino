// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Events;

public record EpisodeAiredEvent(Guid ProjectId, Guid EpisodeId, DateTimeOffset AirTime) : IEvent;
