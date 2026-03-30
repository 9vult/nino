// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Groups.Admins.Add;

public sealed record AddGroupAdminCommand(
    GroupId GroupId,
    UserId UserId,
    UserId RequestedBy,
    bool OverrideVerification
) : ICommand;
