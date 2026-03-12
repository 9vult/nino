// SPDX-License-Identifier: MPL-2.0

using Nino.Localization;

namespace Nino.Domain.Enums;

/// <summary>
/// Controls how response embeds appear
/// </summary>
public enum ProgressResponseType
{
    /// <summary>
    /// Responses to progress commands will not include a status report
    /// </summary>
    [LocalizationKey("choice.progressResponse.type.succinct")]
    Succinct = 0,

    /// <summary>
    /// Responses to progress commands will include a status report
    /// </summary>
    [LocalizationKey("choice.progressResponse.type.verbose")]
    Verbose = 1,
}
