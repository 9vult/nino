// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

/// <summary>
/// Controls how published progress embeds appear
/// </summary>
public enum ProgressPublishType
{
    /// <summary>
    /// Published progress updates will show task abbreviations
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Published progress updates will show full task names
    /// </summary>
    Extended = 1,
}

public static class ProgressPublishTypeExtensions
{
    public static string ToFriendlyString(this ProgressPublishType type, string lng)
    {
        return type switch
        {
            ProgressPublishType.Normal => T("choice.server.display.type.normal", lng),
            ProgressPublishType.Extended => T("choice.server.display.type.extended", lng),
            _ => type.ToString(),
        };
    }
}
