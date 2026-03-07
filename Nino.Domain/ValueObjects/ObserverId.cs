// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct ObserverId(Guid Value) : IId<ObserverId>
{
    public static ObserverId New() => new(Guid.NewGuid());

    public static ObserverId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
