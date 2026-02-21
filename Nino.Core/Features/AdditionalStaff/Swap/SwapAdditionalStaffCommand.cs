// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.AdditionalStaff.Swap;

public sealed record SwapAdditionalStaffCommand(
    Guid ProjectId,
    string EpisodeNumber,
    Guid UserId,
    string Abbreviation,
    Guid RequestedBy
);
