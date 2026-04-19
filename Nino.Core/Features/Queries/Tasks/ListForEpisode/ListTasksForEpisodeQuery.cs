// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.ListForEpisode;

public sealed record ListTasksForEpisodeQuery(EpisodeId EpisodeId) : IQuery;
