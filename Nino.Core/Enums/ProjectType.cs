// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

/// <summary>
/// Type of project
/// </summary>
// ReSharper disable InconsistentNaming
public enum ProjectType
{
    /// <summary>
    /// Multi-episode TV series
    /// </summary>
    TV = 0,

    /// <summary>
    /// Movie
    /// </summary>
    Movie = 1,

    // BD = 2
    /// <summary>
    /// Original Video Animation
    /// </summary>
    OVA = 3,

    /// <summary>
    /// Original Net Animation
    /// </summary>
    ONA = 4,

    /// <summary>
    /// Something else
    /// </summary>
    Other = 5,
}

public static class ProjectTypeExtensions
{
    public static string ToFriendlyString(this ProjectType type, string lng)
    {
        return type switch
        {
            ProjectType.TV => T("choice.project.type.tv", lng),
            ProjectType.Movie => T("choice.project.type.movie", lng),
            ProjectType.OVA => T("choice.project.type.ova", lng),
            ProjectType.ONA => T("choice.project.type.ona", lng),
            ProjectType.Other => T("choice.project.type.other", lng),
            _ => type.ToString(),
        };
    }
}
