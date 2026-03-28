// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.TemplateStaff.Edit;

public sealed record EditTemplateStaffCommand(
    ProjectId ProjectId,
    TemplateStaffId TemplateStaffId,
    UserId RequestedBy,
    TemplateStaffApplicator Applicator,
    UserId? AssigneeId = null,
    Abbreviation? Abbreviation = null,
    string? Name = null,
    decimal? Weight = null,
    bool? IsPseudo = null
) : ICommand;
