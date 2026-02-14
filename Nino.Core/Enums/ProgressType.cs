// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

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

public static class ProgressTypeExtensions
{
    public static string ToFriendlyString(this ProgressType type, string lng)
    {
        return type switch
        {
            ProgressType.Done => T("choice.progress.type.done", lng),
            ProgressType.Undone => T("choice.progress.type.undone", lng),
            ProgressType.Skipped => T("choice.progress.type.skipped", lng),
            _ => type.ToString(),
        };
    }
}
