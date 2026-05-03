// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Tasks.BulkMark;

public sealed record BulkMarkTasksResponse(List<(EpisodeId, Number)> CompletedEpisodes);
