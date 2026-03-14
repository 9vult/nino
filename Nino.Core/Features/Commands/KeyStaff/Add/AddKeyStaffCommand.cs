// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.KeyStaff.Add;

public sealed record AddKeyStaffCommand(
    ProjectId ProjectId,
    UserId RequestedBy,
    string Abbreviation,
    string FullName,
    UserId MemberId,
    bool IsPseudo,
    bool MarkDoneForDoneEpisodes
);
