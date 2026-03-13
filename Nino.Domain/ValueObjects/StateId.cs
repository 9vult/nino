// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct StateId(Guid Value) : IId<StateId>
{
    public static StateId New() => new(Guid.NewGuid());

    public static StateId From(Guid value) => new(value);

    public static StateId From(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();
}
