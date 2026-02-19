// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Project.Resolve;

public sealed record ResolveProjectQuery(
    string Alias,
    Guid GroupId,
    Guid RequestedBy,
    bool IncludeObservers = false
);
