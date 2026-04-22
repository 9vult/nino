// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Tasks.Import;

public sealed record ImportTaskCommand(ProjectId ProjectId, string Data, UserId RequestedBy)
    : ICommand;
