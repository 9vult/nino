// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Task.Resolve;

public sealed record ResolveTaskQuery(EpisodeId EpisodeId, string Abbreviation);
