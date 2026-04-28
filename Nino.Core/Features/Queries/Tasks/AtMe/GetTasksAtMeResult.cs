// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.AtMe;

public sealed record GetTasksAtMeResult(
    ProjectId ProjectId,
    Alias ProjectAlias,
    Number EpisodeNumber,
    string TaskName,
    decimal Weight,
    bool IsPseudo,
    AniListId AniListId
);
