// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record CustomReleasedObserverEvent(
    ProjectId ProjectId,
    ObserverId ObserverId,
    string? Label,
    List<string> Urls,
    string? Commentary,
    bool Publish
) : IEvent;
