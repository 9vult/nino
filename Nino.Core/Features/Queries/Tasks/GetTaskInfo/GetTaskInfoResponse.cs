// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.GetTaskInfo;

public sealed record GetTaskInfoResponse(
    TaskId TaskId,
    Abbreviation TaskAbbreviation,
    string TaskName,
    EpisodeId EpisodeId,
    Number EpisodeNumber
);
