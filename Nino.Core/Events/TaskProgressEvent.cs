// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record TaskProgressEvent(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    TaskId TaskId,
    ProgressType ProgressType
) : IEvent;
