// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.AtMe;

public sealed record GetTasksAtMeQuery(
    UserId RequestedBy,
    GroupId GroupId,
    AtMeType Type,
    bool Global,
    bool IncludePseudo,
    int? Page
) : IQuery;
