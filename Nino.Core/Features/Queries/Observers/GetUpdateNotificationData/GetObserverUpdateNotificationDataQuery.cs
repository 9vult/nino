// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Observers.GetUpdateNotificationData;

public sealed record GetObserverUpdateNotificationDataQuery(ObserverId ObserverId, TaskId TaskId)
    : IQuery;
