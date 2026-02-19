// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.KeyStaff.Add;

public sealed record AddKeyStaffCommand(
    Guid ProjectId,
    Guid UserId,
    string Abbreviation,
    string FullName,
    bool IsPseudo,
    Guid RequestedBy
);
