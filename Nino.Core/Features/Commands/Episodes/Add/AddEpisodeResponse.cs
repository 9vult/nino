// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.Episodes.Add;

public record AddEpisodeResponse(
    string ProjectTitle,
    ProjectType ProjectType,
    string PosterUrl,
    int AddedEpisodeCount
);
