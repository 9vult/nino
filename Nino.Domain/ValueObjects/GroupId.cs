// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct GroupId(Guid Value) : IId<GroupId>
{
    public static GroupId New() => new(Guid.NewGuid());

    public static GroupId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
