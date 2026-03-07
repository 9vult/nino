// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct AliasId(Guid Value) : IId<AliasId>
{
    public static AliasId New() => new(Guid.NewGuid());

    public static AliasId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
