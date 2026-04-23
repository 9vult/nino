// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Observers.ListNames;

public sealed record ListObserverNamesQuery(
    GroupId GroupId,
    UserId RequestedBy,
    bool OverrideVerification
) : IQuery;
