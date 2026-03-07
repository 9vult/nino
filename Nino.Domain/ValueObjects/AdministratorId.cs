// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct AdministratorId(Guid Value) : IId<AdministratorId>
{
    public static AdministratorId New() => new(Guid.NewGuid());

    public static AdministratorId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
