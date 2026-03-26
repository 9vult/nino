// SPDX-License-Identifier: MPL-2.0

using Vogen;

namespace Nino.Domain.ValueObjects;

[ValueObject<Guid>]
[Instance("Unset", "Guid.Empty")]
public readonly partial struct ChannelId;
