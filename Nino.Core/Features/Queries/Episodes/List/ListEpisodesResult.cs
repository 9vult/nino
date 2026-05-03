// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.List;

public sealed record ListEpisodesResult(EpisodeId Id, Number Number);
