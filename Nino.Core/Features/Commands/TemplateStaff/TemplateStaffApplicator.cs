// SPDX-License-Identifier: MPL-2.0

using Nino.Localization;

namespace Nino.Core.Features.Commands.TemplateStaff;

public enum TemplateStaffApplicator
{
    /// <summary>
    /// Changes will be applied to all existing episodes and future episodes
    /// </summary>
    [LocalizationKey("choice.applicator.type.allEpisodes")]
    AllEpisodes,

    /// <summary>
    /// Changes will be applied to existing incomplete episodes and future episodes
    /// </summary>
    [LocalizationKey("choice.applicator.type.incompleteEpisodes")]
    IncompleteEpisodes,

    /// <summary>
    /// Changes will be applied to future episodes (No changes to existing episodes)
    /// </summary>
    [LocalizationKey("choice.applicator.type.futureEpisodes")]
    FutureEpisodes,
}
