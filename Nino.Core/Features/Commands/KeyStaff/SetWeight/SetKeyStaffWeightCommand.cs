// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.KeyStaff.SetWeight;

public sealed record SetKeyStaffWeightCommand(
    ProjectId ProjectId,
    StaffId StaffId,
    UserId RequestedBy,
    decimal NewWeight
);
