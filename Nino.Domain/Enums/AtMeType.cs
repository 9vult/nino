// SPDX-License-Identifier: MPL-2.0

using Nino.Localization;

namespace Nino.Domain.Enums;

/// <summary>
/// Filter for At Me results
/// </summary>
public enum AtMeType
{
    /// <summary>
    /// Automatically choose between <see cref="Conga"/> and <see cref="Incomplete"/>
    /// </summary>
    [LocalizationKey("choice.atMe.type.auto")]
    Auto = 0,

    /// <summary>
    /// Use project Conga lines to determine if the task is At You
    /// </summary>
    [LocalizationKey("choice.atMe.type.conga")]
    Conga = 1,

    /// <summary>
    /// Use task completion status to determine if the task is At You
    /// </summary>
    [LocalizationKey("choice.atMe.type.incomplete")]
    Incomplete = 2,
}
