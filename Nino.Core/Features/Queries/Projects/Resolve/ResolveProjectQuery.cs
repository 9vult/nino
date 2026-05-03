// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.Resolve;

public sealed record ResolveProjectQuery(
    Alias Alias,
    GroupId GroupId,
    UserId RequestedBy,
    bool IncludeObservers = false
) : IQuery;
