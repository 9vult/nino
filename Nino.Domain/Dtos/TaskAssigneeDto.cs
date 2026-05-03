// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Dtos;

public sealed record TaskAssigneeDto(string TaskName, MappedIdDto<UserId> Assignee);
