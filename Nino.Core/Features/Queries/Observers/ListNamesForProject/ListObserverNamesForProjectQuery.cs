// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Observers.ListNamesForProject;

public sealed record ListObserverNamesForProjectQuery(ProjectId ProjectId, UserId RequestedBy)
    : IQuery;
