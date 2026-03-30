// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Groups.Admins.Remove;

public sealed record RemoveGroupAdminCommand(
    GroupId GroupId,
    UserId UserId,
    UserId RequestedBy,
    bool OverrideVerification
) : ICommand;
