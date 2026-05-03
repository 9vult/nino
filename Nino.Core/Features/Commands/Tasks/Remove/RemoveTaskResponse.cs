// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Tasks.Remove;

public sealed record RemoveTaskResponse(List<(EpisodeId, Number)> CompletedEpisodes);
