// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.Episodes.Remove;

public record RemoveEpisodeResponse(
    string ProjectTitle,
    ProjectType ProjectType,
    string PosterUrl,
    int RemovedEpisodeCount
);
