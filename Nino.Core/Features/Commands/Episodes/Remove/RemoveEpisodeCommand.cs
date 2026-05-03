// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Episodes.Remove;

public sealed record RemoveEpisodeCommand(
    ProjectId ProjectId,
    EpisodeId FirstEpisodeId,
    EpisodeId LastEpisodeId,
    UserId RequestedBy
) : ICommand;
