// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.GetCongaNotificationData;

public sealed record GetCongaNotificationDataQuery(
    EpisodeId EpisodeId,
    IReadOnlyList<TaskId> TaskIds
) : IQuery;
