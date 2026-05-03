// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;

namespace Nino.Core.Features.Queries.Tasks.AtMe;

public sealed record GetTasksAtMeResponse(
    IReadOnlyList<GetTasksAtMeResult> Results,
    AtMeType Type,
    int Page,
    int PageCount
);
