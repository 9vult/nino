// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.BlameAll;

public sealed record BlameAllQuery(
    ProjectId ProjectId,
    BlameAllFilter Filter,
    bool IncludePseudo,
    int? Page,
    UserId RequestedBy
) : IQuery;
