// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

/// <summary>
/// Type of report to generate for Blame All
/// </summary>
public enum BlameAllType
{
    /// <summary>
    /// Display the progress report for each episode
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Display the last-modified time for each episode
    /// </summary>
    StallCheck = 1,
}

public static class BlameAllTypeExtensions
{
    public static string ToFriendlyString(this BlameAllType type, string lng)
    {
        return type switch
        {
            BlameAllType.Normal => T("choice.blameAll.type.normal", lng),
            BlameAllType.StallCheck => T("choice.blameAll.type.stallCheck", lng),
            _ => type.ToString(),
        };
    }
}
