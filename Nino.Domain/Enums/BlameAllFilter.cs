// SPDX-License-Identifier: MPL-2.0

using Nino.Localization;

namespace Nino.Domain.Enums;

/// <summary>
/// Filter for Blame All results
/// </summary>
public enum BlameAllFilter
{
    /// <summary>
    /// Display all episodes
    /// </summary>
    [LocalizationKey("choice.blameAll.type.all")]
    All = 0,

    /// <summary>
    /// Only display episodes that have completed tasks, but are not complete
    /// </summary>
    [LocalizationKey("choice.blameAll.type.inProgress")]
    InProgress = 1,

    /// <summary>
    /// Only display episodes that have not been completed
    /// </summary>
    [LocalizationKey("choice.blameAll.type.incomplete")]
    Incomplete = 2,
}
