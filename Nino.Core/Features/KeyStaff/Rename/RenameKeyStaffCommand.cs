// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.KeyStaff.Rename;

public sealed record RenameKeyStaffCommand(
    Guid ProjectId,
    string Abbreviation,
    string NewAbbreviation,
    string FullName,
    Guid RequestedBy
);
