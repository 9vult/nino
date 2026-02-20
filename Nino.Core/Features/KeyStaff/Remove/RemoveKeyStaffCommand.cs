// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.KeyStaff.Remove;

public sealed record RemoveKeyStaffCommand(Guid ProjectId, string Abbreviation, Guid RequestedBy);
