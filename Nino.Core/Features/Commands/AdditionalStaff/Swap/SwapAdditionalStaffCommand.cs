// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.AdditionalStaff.Swap;

public sealed record SwapAdditionalStaffCommand(
    ProjectId ProjectId,
    StaffId StaffId,
    UserId RequestedBy,
    UserId MemberId
);
