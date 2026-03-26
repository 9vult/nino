// SPDX-License-Identifier: MPL-2.0

using Vogen;

namespace Nino.Domain.ValueObjects;

[ValueObject<int>]
[Instance("Unset", 0)]
public readonly partial struct AniListId
{
    private static Validation Validate(int input)
    {
        return input >= 0
            ? Validation.Ok
            : Validation.Invalid("AniListId must be greater than or equal to 0");
    }
}
