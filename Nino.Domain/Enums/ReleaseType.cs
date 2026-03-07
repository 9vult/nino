// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Enums;

/// <summary>
/// Type of release
/// </summary>
public enum ReleaseType
{
    /// <summary>
    /// A single episode
    /// </summary>
    Episode = 0,

    /// <summary>
    /// A volume (e.g. BD volume)
    /// </summary>
    Volume = 1,

    /// <summary>
    /// A group of episodes, typically an entire series or cour
    /// </summary>
    Batch = 2,
}
