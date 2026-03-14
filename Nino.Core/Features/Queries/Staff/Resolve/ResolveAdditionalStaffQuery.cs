// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Staff.Resolve;

public sealed record ResolveAdditionalStaffQuery(EpisodeId EpisodeId, string Abbreviation);
