// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.List;

public sealed record ListProjectsResult(
    Alias Nickname,
    string OwnerName,
    bool IsPrivate,
    bool IsArchived,
    int EpisodeCount,
    int ObserverCount,
    bool HasDelegateObserver,
    bool AirNotificationsEnabled,
    bool CongaRemindersEnabled,
    int TotalTaskCount,
    int TotalTaskCompletedCount,
    int TotalNonPseudoTaskCount,
    int TotalNonPseudoTaskCompletedCount
);
