// SPDX-License-Identifier: MPL-2.0

using Nino.Localization;

namespace Nino.Domain.Enums;

/// <summary>
/// Prefix to use for Conga reminders
/// </summary>
public enum CongaPrefixType
{
    /// <summary>
    /// No prefix will be prepended to the reminder
    /// </summary>
    [LocalizationKey("choice.congaPrefixType.none")]
    None = 0,

    /// <summary>
    /// The project's nickname will be prepended to the reminder
    /// </summary>
    [LocalizationKey("choice.congaPrefixType.nickname")]
    Nickname = 1,

    /// <summary>
    /// The project's full title will be prepended to the reminder
    /// </summary>
    [LocalizationKey("choice.congaPrefixType.title")]
    Title = 2,
}
