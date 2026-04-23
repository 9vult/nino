// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Observers.Remove;

public sealed record RemoveObserverCommand(
    ObserverId ObserverId,
    UserId RequestedBy,
    bool OverrideVerification
) : ICommand;
