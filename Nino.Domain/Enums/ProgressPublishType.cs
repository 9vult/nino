// SPDX-License-Identifier: MPL-2.0

using Nino.Localization;

namespace Nino.Domain.Enums;

/// <summary>
/// Controls how published progress embeds appear
/// </summary>
public enum ProgressPublishType
{
    /// <summary>
    /// Published progress updates will show task abbreviations
    /// </summary>
    [LocalizationKey("choice.progressPublish.type.normal")]
    Normal = 0,

    /// <summary>
    /// Published progress updates will show full task names
    /// </summary>
    [LocalizationKey("choice.progressPublish.type.extended")]
    Extended = 1,
}
