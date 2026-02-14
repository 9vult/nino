// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

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

public static class ReleaseTypeExtensions
{
    public static string ToFriendlyString(this ReleaseType type, string lng)
    {
        return type switch
        {
            ReleaseType.Episode => T("choice.release.type.episode", lng),
            ReleaseType.Volume => T("choice.release.type.volume", lng),
            ReleaseType.Batch => T("choice.release.type.batch", lng),
            _ => type.ToString(),
        };
    }
}
