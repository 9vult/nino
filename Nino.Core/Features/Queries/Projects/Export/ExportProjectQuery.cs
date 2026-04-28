// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.Export;

public sealed record ExportProjectQuery(
    ProjectId ProjectId,
    UserId RequestedBy,
    bool OverrideVerification
) : IQuery;
