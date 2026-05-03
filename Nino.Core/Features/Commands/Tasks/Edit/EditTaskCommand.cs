// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Tasks.Edit;

public sealed record EditTaskCommand(
    ProjectId ProjectId,
    EpisodeId FirstEpisodeId,
    EpisodeId LastEpisodeId,
    Abbreviation Abbreviation,
    UserId RequestedBy,
    UserId? AssigneeId = null,
    Abbreviation? NewAbbreviation = null,
    string? Name = null,
    decimal? Weight = null,
    bool? IsPseudo = null
) : ICommand;
