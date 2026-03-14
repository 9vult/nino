// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.KeyStaff.Swap;

public sealed record SwapKeyStaffCommand(
    ProjectId ProjectId,
    StaffId StaffId,
    UserId RequestedBy,
    UserId MemberId
);
