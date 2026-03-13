// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.KeyStaff.Add;

public record AddKeyStaffCommand(
    ProjectId ProjectId,
    UserId RequestedBy,
    bool MarkDoneForDoneEpisodes
)
{
    public required string Abbreviation { get; set; }
    public required string FullName { get; set; }
    public required UserId MemberId { get; set; }
    public required bool IsPseudo { get; set; }
}
