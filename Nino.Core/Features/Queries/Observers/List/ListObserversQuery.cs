// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Observers.List;

public sealed record ListObserversQuery(GroupId GroupId, UserId RequestedBy) : IQuery;
