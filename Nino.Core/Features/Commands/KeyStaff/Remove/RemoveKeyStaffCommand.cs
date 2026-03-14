// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.KeyStaff.Remove;

public sealed record RemoveKeyStaffCommand(
    ProjectId ProjectId,
    StaffId StaffId,
    UserId RequestedBy
);
