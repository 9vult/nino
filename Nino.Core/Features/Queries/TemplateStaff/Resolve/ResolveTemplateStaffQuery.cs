// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.TemplateStaff.Resolve;

public sealed record ResolveTemplateStaffQuery(ProjectId ProjectId, Abbreviation Abbreviation)
    : IQuery;
