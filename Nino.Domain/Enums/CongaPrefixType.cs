// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Enums;

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
