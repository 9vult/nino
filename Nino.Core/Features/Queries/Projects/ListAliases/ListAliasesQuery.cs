// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.ListAliases;

public sealed record ListAliasesQuery(
    UserId RequestedBy,
    GroupId GroupId,
    bool IncludeObservers,
    bool IncludeArchived
) : IQuery;
