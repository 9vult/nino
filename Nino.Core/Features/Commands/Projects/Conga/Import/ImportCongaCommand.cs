// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Conga.Import;

public sealed record ImportCongaCommand(ProjectId ProjectId, string Data, UserId RequestedBy)
    : ICommand;
