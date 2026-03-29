// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Delete;

public sealed record DeleteProjectCommand(ProjectId ProjectId, UserId RequestedBy) : ICommand;
