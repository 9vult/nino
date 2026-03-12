// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct AniListId(int Value)
{
    public static AniListId From(int value) => new(value);

    public override string ToString() => Value.ToString();
}
