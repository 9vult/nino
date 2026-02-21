// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.AdditionalStaff.Rename;

public sealed record RenameAdditionalStaffCommand(
    Guid ProjectId,
    string EpisodeNumber,
    string Abbreviation,
    string NewAbbreviation,
    string FullName,
    Guid RequestedBy
);
