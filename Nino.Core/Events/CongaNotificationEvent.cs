// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record CongaNotificationEvent(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    IReadOnlyList<TaskId> TaskIds,
    bool IsReminder
) : IEvent;
