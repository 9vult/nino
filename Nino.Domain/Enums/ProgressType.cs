// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Enums;

/// <summary>
/// Type of progress afflicting a <see cref="Task"/>
/// </summary>
public enum ProgressType
{
    /// <summary>
    /// Task was completed
    /// </summary>
    Done = 0,

    /// <summary>
    /// Task was marked incomplete
    /// </summary>
    Undone = 1,

    /// <summary>
    /// Task was skipped
    /// </summary>
    Skipped = 2,
}
