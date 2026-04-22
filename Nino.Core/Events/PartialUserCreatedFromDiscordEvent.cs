// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Events;

public sealed record PartialUserCreatedFromDiscordEvent(UserId UserId, ulong DiscordId) : IEvent;
