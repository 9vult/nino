// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.GetWorkingEpisode;

public sealed record GetWorkingEpisodeResponse(EpisodeId EpisodeId, Number EpisodeNumber);
