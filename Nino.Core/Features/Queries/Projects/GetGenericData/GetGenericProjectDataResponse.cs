// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.GetGenericData;

/// <summary>
/// A generic response containing information about the project
/// </summary>
/// <param name="ProjectId">Project's ID</param>
/// <param name="ProjectTitle">Full project title</param>
/// <param name="ProjectType">Type of project</param>
/// <param name="PosterUrl">Project poster URL</param>
/// <param name="AniListUrl">AniList URL</param>
public record GetGenericProjectDataResponse(
    ProjectId ProjectId,
    string ProjectTitle,
    ProjectType ProjectType,
    AniListId AniListId,
    string PosterUrl,
    string AniListUrl,
    bool IsPrivate
);
