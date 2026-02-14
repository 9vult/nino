// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

/// <summary>
/// Prefix to use for Conga reminders
/// </summary>
public enum CongaPrefixType
{
    /// <summary>
    /// No prefix will be prepended to the reminder
    /// </summary>
    None = 0,

    /// <summary>
    /// The project's nickname will be prepended to the reminder
    /// </summary>
    Nickname = 1,

    /// <summary>
    /// The project's full title will be prepended to the reminder
    /// </summary>
    Title = 2,
}

public static class CongaPrefixTypeExtensions
{
    public static string ToFriendlyString(this CongaPrefixType type, string lng)
    {
        return type switch
        {
            CongaPrefixType.None => T("choice.server.congaPrefixType.none", lng),
            CongaPrefixType.Nickname => T("choice.server.congaPrefixType.nickname", lng),
            CongaPrefixType.Title => T("choice.server.congaPrefixType.title", lng),
            _ => type.ToString(),
        };
    }
}
