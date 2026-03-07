// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct PinchHitterId(Guid Value) : IId<PinchHitterId>
{
    public static PinchHitterId New() => new(Guid.NewGuid());

    public static PinchHitterId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
