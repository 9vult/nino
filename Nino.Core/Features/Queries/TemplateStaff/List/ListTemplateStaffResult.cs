// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.TemplateStaff.List;

public sealed record ListTemplateStaffResult(TemplateStaffId Id, Abbreviation Abbreviation);
