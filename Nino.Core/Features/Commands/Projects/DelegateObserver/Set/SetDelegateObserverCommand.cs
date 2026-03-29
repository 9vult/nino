// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.DelegateObserver.Set;

public sealed record SetDelegateObserverCommand(
    ProjectId ProjectId,
    ObserverId ObserverId,
    UserId RequestedBy
) : ICommand;
