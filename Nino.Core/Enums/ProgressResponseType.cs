// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

/// <summary>
/// Controls how response embeds appear
/// </summary>
public enum ProgressResponseType
{
    /// <summary>
    /// Responses to progress commands will not include a status report
    /// </summary>
    Succinct = 0,

    /// <summary>
    /// Responses to progress commands will include a status report
    /// </summary>
    Verbose = 1,
}

public static class ProgressResponseTypeExtensions
{
    public static string ToFriendlyString(this ProgressResponseType type, string lng)
    {
        return type switch
        {
            ProgressResponseType.Succinct => T("choice.progressResponse.type.succinct", lng),
            ProgressResponseType.Verbose => T("choice.progressResponse.type.verbose", lng),
            _ => type.ToString(),
        };
    }
}
