// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Archive;

public sealed record ArchiveProjectCommand(ProjectId ProjectId, UserId RequestedBy) : ICommand;
