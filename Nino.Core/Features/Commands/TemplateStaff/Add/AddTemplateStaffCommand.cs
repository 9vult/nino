// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.TemplateStaff.Add;

public sealed record AddTemplateStaffCommand(
    ProjectId ProjectId,
    UserId RequestedBy,
    TemplateStaffApplicator Applicator,
    UserId AssigneeId,
    Abbreviation Abbreviation,
    string Name,
    bool IsPseudo
) : ICommand;
