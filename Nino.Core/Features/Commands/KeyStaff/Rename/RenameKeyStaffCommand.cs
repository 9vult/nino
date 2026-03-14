// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.KeyStaff.Rename;

public sealed record RenameKeyStaffCommand(
    ProjectId ProjectId,
    StaffId StaffId,
    UserId RequestedBy,
    string NewAbbreviation,
    string NewName
);
