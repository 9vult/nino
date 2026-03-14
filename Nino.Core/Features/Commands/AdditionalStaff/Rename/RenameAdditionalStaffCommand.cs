// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.AdditionalStaff.Rename;

public sealed record RenameAdditionalStaffCommand(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    StaffId StaffId,
    UserId RequestedBy,
    string NewAbbreviation,
    string NewName
);
