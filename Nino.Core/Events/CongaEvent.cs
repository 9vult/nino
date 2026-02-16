// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Events;

public record CongaEvent(Guid ProjectId, Guid EpisodeId, Guid TaskId) : IEvent;
