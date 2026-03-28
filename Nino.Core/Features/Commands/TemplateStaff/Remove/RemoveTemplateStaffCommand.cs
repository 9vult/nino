// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.TemplateStaff.Remove;

public sealed record RemoveTemplateStaffCommand(
    ProjectId ProjectId,
    TemplateStaffId TemplateStaffId,
    TemplateStaffApplicator Applicator,
    UserId RequestedBy
) : ICommand;
