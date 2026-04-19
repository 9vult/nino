// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.Resolve;

public sealed record ResolveEpisodeQuery(ProjectId ProjectId, Number Number) : IQuery;
