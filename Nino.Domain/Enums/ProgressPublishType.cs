// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Enums;

/// <summary>
/// Controls how published progress embeds appear
/// </summary>
public enum ProgressPublishType
{
    /// <summary>
    /// Published progress updates will show task abbreviations
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Published progress updates will show full task names
    /// </summary>
    Extended = 1,
}
