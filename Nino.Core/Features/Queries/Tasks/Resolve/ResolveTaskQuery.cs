// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.Resolve;

public sealed record ResolveTaskQuery(EpisodeId EpisodeId, Abbreviation Abbreviation) : IQuery;
