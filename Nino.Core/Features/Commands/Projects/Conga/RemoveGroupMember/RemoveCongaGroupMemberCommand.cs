// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Conga.RemoveGroupMember;

public sealed record RemoveCongaGroupMemberCommand(
    ProjectId ProjectId,
    Abbreviation GroupName,
    Abbreviation NodeName,
    UserId RequestedBy
) : ICommand;
