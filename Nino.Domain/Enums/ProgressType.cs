// SPDX-License-Identifier: MPL-2.0

using Nino.Localization;

namespace Nino.Domain.Enums;

/// <summary>
/// Type of progress afflicting a <see cref="Task"/>
/// </summary>
public enum ProgressType
{
    /// <summary>
    /// Task was completed
    /// </summary>
    [LocalizationKey("choice.progress.type.done")]
    Done = 0,

    /// <summary>
    /// Task was marked incomplete
    /// </summary>
    [LocalizationKey("choice.progress.type.undone")]
    Undone = 1,

    /// <summary>
    /// Task was skipped
    /// </summary>
    [LocalizationKey("choice.progress.type.skipped")]
    Skipped = 2,
}
