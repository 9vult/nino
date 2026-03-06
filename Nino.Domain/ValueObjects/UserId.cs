// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct UserId(Guid Value) : IId<UserId>
{
    public static UserId New() => new(Guid.NewGuid());

    public static UserId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
