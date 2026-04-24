// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Observers.ListNamesForGroup;

public sealed record ListObserverNamesForGroupQuery(
    GroupId GroupId,
    UserId RequestedBy,
    bool OverrideVerification
) : IQuery;
