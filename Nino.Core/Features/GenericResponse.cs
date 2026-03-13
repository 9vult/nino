// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;

namespace Nino.Core.Features;

/// <summary>
/// A generic task response containing information about the affected project
/// </summary>
/// <param name="ProjectTitle">Full project title</param>
/// <param name="ProjectType">Type of project</param>
/// <param name="PosterUrl">Project poster URL</param>
public record GenericResponse(string ProjectTitle, ProjectType ProjectType, string PosterUrl);
