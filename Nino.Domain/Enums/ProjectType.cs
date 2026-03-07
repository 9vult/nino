// SPDX-License-Identifier: MPL-2.0

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
