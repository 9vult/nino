// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

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

public static class BlameAllFilterExtensions
{
    public static string ToFriendlyString(this BlameAllFilter type, string lng)
    {
        return type switch
        {
            BlameAllFilter.All => T("choice.blameAll.type.all", lng),
            BlameAllFilter.InProgress => T("choice.blameAll.type.inProgress", lng),
            BlameAllFilter.Incomplete => T("choice.blameAll.type.incomplete", lng),
            _ => type.ToString(),
        };
    }
}
