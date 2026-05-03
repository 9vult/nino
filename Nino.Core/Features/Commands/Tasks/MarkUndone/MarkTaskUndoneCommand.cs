// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Tasks.MarkUndone;

public sealed record MarkTaskUndoneCommand(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    TaskId TaskId,
    UserId RequestedBy
) : ICommand;
