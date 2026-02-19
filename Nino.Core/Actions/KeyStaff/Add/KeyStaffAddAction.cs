// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Actions.KeyStaff.Add;

public sealed record KeyStaffAddAction(
    Guid ProjectId,
    Guid UserId,
    string Abbreviation,
    string FullName,
    bool IsPseudo,
    Guid RequestedBy
);
