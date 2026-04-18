// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Tasks.BulkMark;

public sealed record BulkMarkTasksCommand(
    ProjectId ProjectId,
    EpisodeId FirstEpisodeId,
    EpisodeId LastEpisodeId,
    Abbreviation Abbreviation,
    ProgressType ProgressType,
    UserId RequestedBy
) : ICommand;
