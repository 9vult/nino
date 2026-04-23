// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record VolumeReleasedObserverEvent(
    ProjectId ProjectId,
    ObserverId ObserverId,
    Number Number,
    List<string> Urls,
    bool Publish
) : IEvent;
