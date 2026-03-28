// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Tasks.Add;

public sealed record AddTaskCommand(
    ProjectId ProjectId,
    EpisodeId FirstEpisodeId,
    EpisodeId LastEpisodeId,
    UserId RequestedBy,
    UserId AssigneeId,
    Abbreviation Abbreviation,
    string Name,
    bool IsPseudo
) : ICommand;
