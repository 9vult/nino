// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.KeyStaff.Swap;

public sealed record SwapKeyStaffCommand(
    Guid ProjectId,
    Guid UserId,
    string Abbreviation,
    Guid RequestedBy
);
