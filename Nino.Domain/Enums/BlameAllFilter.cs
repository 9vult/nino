// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Enums;

/// <summary>
/// Filter for Blame All results
/// </summary>
public enum BlameAllFilter
{
    /// <summary>
    /// Display all episodes
    /// </summary>
    All = 0,

    /// <summary>
    /// Only display episodes that have completed tasks, but are not complete
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Only display episodes that have not been completed
    /// </summary>
    Incomplete = 2,
}
