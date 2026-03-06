// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct MentionRoleId(Guid Value)
{
    public static MentionRoleId New() => new(Guid.NewGuid());

    public static MentionRoleId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
