// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Discord.Entities;

public sealed record InteractionIdentityResult(UserId UserId, GroupId GroupId);
