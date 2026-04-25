// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Queries.Nino.Statistics;

public sealed record NinoStatisticsResponse(
    int TotalGroups,
    int TotalProjects,
    int TotalEpisodes,
    int CompletedEpisodes,
    int CompletedTasks,
    int TotalObservers,
    int ObserverProjectCount
);
