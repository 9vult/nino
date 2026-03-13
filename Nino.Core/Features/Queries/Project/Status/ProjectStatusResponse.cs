// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Project.Status;

public sealed record ProjectStatusResponse(
    ProjectId ProjectId,
    string ProjectTitle,
    ProjectType ProjectType,
    int EpisodeCount,
    int CompletedEpisodeCount
);
