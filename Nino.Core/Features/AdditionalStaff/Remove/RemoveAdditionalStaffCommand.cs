// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.AdditionalStaff.Remove;

public sealed record RemoveAdditionalStaffCommand(
    Guid ProjectId,
    string EpisodeNumber,
    string Abbreviation,
    Guid RequestedBy
);
