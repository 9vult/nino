// SPDX-License-Identifier: MPL-2.0

using Nino.Localization;

namespace Nino.Domain.Enums;

/// <summary>
/// Type of project
/// </summary>
// ReSharper disable InconsistentNaming
public enum ProjectType
{
    /// <summary>
    /// Multi-episode TV series
    /// </summary>
    [LocalizationKey("choice.project.type.tv")]
    TV = 0,

    /// <summary>
    /// Movie
    /// </summary>
    [LocalizationKey("choice.project.type.movie")]
    Movie = 1,

    // BD = 2

    /// <summary>
    /// Original Video Animation
    /// </summary>
    [LocalizationKey("choice.project.type.ova")]
    OVA = 3,

    /// <summary>
    /// Original Net Animation
    /// </summary>
    [LocalizationKey("choice.project.type.ona")]
    ONA = 4,

    /// <summary>
    /// Something else
    /// </summary>
    [LocalizationKey("choice.project.type.other")]
    Other = 5,
}
