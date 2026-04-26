// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.GetBulkProgressNotificationData;

public sealed record GetBulkProgressNotificationDataQuery(
    ProjectId ProjectId,
    EpisodeId FirstEpisodeId,
    EpisodeId LastEpisodeId,
    Abbreviation Abbreviation
) : IQuery;
