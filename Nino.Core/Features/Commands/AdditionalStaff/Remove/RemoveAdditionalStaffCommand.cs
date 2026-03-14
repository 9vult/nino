// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.AdditionalStaff.Remove;

public sealed record RemoveAdditionalStaffCommand(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    StaffId StaffId,
    UserId RequestedBy
);
