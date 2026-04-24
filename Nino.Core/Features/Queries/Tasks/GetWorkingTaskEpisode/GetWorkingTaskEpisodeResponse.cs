// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.GetWorkingTaskEpisode;

public sealed record GetWorkingTaskEpisodeResponse(
    EpisodeId WorkingEpisodeId,
    EpisodeId TaskEpisodeId,
    Number WorkingEpisodeNumber,
    Number TaskEpisodeNumber,
    TaskId TaskId,
    string TaskName,
    int Difference
);
