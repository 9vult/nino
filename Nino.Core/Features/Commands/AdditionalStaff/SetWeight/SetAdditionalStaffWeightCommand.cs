// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.AdditionalStaff.SetWeight;

public sealed record SetAdditionalStaffWeightCommand(
    ProjectId ProjectId,
    StaffId StaffId,
    UserId RequestedBy,
    decimal NewWeight
);
