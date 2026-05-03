// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.TemplateStaff.Import;

public sealed record ImportTemplateStaffCommand(
    ProjectId ProjectId,
    string Data,
    UserId RequestedBy
) : ICommand;
