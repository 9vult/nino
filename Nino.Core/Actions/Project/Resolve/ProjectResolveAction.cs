// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Actions.Project.Resolve;

public sealed record ProjectResolveAction(
    string Alias,
    Guid GroupId,
    Guid RequestedBy,
    bool IncludeObservers = false
);
