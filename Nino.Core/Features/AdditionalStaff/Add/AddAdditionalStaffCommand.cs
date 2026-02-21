// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.AdditionalStaff.Add;

public sealed record AddAdditionalStaffCommand(
    Guid ProjectId,
    Guid UserId,
    string EpisodeNumber,
    string Abbreviation,
    string FullName,
    bool IsPseudo,
    Guid RequestedBy
);
