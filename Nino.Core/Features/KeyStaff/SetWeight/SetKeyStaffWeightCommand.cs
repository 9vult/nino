// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.KeyStaff.SetWeight;

public sealed record SetKeyStaffWeightCommand(
    Guid ProjectId,
    string Abbreviation,
    decimal Weight,
    Guid RequestedBy
);
