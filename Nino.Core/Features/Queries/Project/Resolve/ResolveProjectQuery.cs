// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Project.Resolve;

public sealed record ResolveProjectQuery(
    string Alias,
    GroupId GroupId,
    UserId RequestedBy,
    bool IncludeObservers = false
);
