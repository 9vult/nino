// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Observers.GetBulkUpdateNotificationData;

public sealed record GetBulkObserverUpdateNotificationDataQuery(
    ObserverId ObserverId,
    ProjectId ProjectId,
    EpisodeId FirstEpisodeId,
    EpisodeId LastEpisodeId,
    Abbreviation Abbreviation
) : IQuery;
