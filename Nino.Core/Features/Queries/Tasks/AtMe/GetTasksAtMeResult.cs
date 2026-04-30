// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.AtMe;

public sealed record GetTasksAtMeResult(
    ProjectId ProjectId,
    Alias Nickname,
    Number EpisodeNumber,
    AniListId AniListId,
    IReadOnlyList<GetTasksAtMeTaskResult> Tasks
);
