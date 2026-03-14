// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Staff.Resolve;

public sealed record ResolveKeyStaffQuery(ProjectId ProjectId, string Abbreviation);
