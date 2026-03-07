// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct ConfigurationId(Guid Value) : IId<ConfigurationId>
{
    public static ConfigurationId New() => new(Guid.NewGuid());

    public static ConfigurationId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
