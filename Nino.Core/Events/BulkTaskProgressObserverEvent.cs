// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record BulkTaskProgressObserverEvent(
    ObserverId ObserverId,
    ProjectId ProjectId,
    EpisodeId FirstEpisodeId,
    EpisodeId LastEpisodeId,
    Abbreviation Abbreviation,
    ProgressType ProgressType
) : IEvent;
