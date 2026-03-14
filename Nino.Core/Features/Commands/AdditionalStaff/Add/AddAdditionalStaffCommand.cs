// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.AdditionalStaff.Add;

public sealed record AddAdditionalStaffCommand(
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    UserId RequestedBy
)
{
    public required string Abbreviation { get; set; }
    public required string FullName { get; set; }
    public required UserId MemberId { get; set; }
    public required bool IsPseudo { get; set; }
}
