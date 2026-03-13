// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episode.Resolve;

public sealed record ResolveEpisodeQuery(ProjectId ProjectId, string Number);
