// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.AdditionalStaff.SetWeight;

public sealed record SetAdditionalStaffWeightCommand(
    Guid ProjectId,
    string EpisodeNumber,
    string Abbreviation,
    decimal Weight,
    Guid RequestedBy
);
