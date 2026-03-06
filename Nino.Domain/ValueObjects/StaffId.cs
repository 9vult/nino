// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct StaffId(Guid Value) : IId<StaffId>
{
    public static StaffId New() => new(Guid.NewGuid());

    public static StaffId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
