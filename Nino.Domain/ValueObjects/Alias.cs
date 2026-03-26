// SPDX-License-Identifier: MPL-2.0

using Vogen;

namespace Nino.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct Alias
{
    private static string NormalizeInput(string input)
    {
        return input.Trim().ToLowerInvariant().Replace(" ", string.Empty);
    }

    private static Validation Validate(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && input.Length is > 0 and <= Length.Alias
            ? Validation.Ok
            : Validation.Invalid("Aliases must follow the constraints!");
    }
}
