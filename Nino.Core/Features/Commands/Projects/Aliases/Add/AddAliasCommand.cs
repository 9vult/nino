// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Aliases.Add;

public sealed record AddAliasCommand(ProjectId ProjectId, Alias Alias, UserId RequestedBy)
    : ICommand;
