// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record BatchReleasedObserverEvent(
    ProjectId ProjectId,
    ObserverId ObserverId,
    Number FirstNumber,
    Number LastNumber,
    List<string> Urls,
    bool Publish
) : IEvent;
