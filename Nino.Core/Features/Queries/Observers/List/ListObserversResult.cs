// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Observers.List;

public sealed record ListObserversResult(
    string GroupName,
    Alias ProjectNickname,
    string OwnerName,
    bool IsDelegate
);
