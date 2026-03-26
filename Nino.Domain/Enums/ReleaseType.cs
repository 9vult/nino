// SPDX-License-Identifier: MPL-2.0

using Nino.Localization;

namespace Nino.Domain.Enums;

/// <summary>
/// Type of release
/// </summary>
public enum ReleaseType
{
    /// <summary>
    /// A single episode
    /// </summary>
    [LocalizationKey("choice.release.type.episode")]
    Episode = 0,

    /// <summary>
    /// A volume (e.g. BD volume)
    /// </summary>
    [LocalizationKey("choice.release.type.volume")]
    Volume = 1,

    /// <summary>
    /// A group of episodes, typically an entire series or cour
    /// </summary>
    [LocalizationKey("choice.release.type.batch")]
    Batch = 2,
}
