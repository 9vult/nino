// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Queries.Tasks.AtMe;

public sealed record GetTasksAtMeResponse(
    IReadOnlyList<GetTasksAtMeResult> Results,
    int Page,
    int PageCount
);
