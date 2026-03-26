// SPDX-License-Identifier: MPL-2.0

using Vogen;

namespace Nino.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct Abbreviation
{
    private static string NormalizeInput(string input)
    {
        return input.Trim().ToUpperInvariant();
    }

    private static Validation Validate(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && input.Length is > 0 and <= Length.Abbreviation
            ? Validation.Ok
            : Validation.Invalid("Abbreviations must follow the constraints!");
    }
}
