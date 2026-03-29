// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.DelegateObserver.Remove;

public sealed record RemoveDelegateObserverCommand(ProjectId ProjectId, UserId RequestedBy)
    : ICommand;
