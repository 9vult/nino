// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Events;

public record CongaNotificationEvent(
    Guid ProjectId,
    Guid EpisodeId,
    string Abbreviation,
    bool IsReminder
) : IEvent;
