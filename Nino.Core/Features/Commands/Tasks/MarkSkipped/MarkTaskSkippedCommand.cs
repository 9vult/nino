// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Tasks.MarkSkipped;

public sealed record MarkTaskSkippedCommand(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    TaskId TaskId,
    UserId RequestedBy
) : ICommand;
