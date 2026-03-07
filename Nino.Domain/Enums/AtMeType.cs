// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Enums;

/// <summary>
/// Filter for At Me results
/// </summary>
public enum AtMeType
{
    /// <summary>
    /// Automatically choose between <see cref="Conga"/> and <see cref="Incomplete"/>
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Use project Conga lines to determine if the task is At You
    /// </summary>
    Conga = 1,

    /// <summary>
    /// Use task completion status to determine if the task is At You
    /// </summary>
    Incomplete = 2,
}
