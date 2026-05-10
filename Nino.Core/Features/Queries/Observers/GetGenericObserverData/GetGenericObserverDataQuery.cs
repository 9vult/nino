// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Observers.GetGenericObserverData;

public sealed record GetGenericObserverDataQuery(ObserverId ObserverId) : IQuery;
