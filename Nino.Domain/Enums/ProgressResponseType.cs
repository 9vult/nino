// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Enums;

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
