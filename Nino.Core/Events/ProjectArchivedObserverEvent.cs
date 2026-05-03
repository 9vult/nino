// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record ProjectArchivedObserverEvent(ObserverId ObserverId, ProjectId ProjectId)
    : IEvent;
