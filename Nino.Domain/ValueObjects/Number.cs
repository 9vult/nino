// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using Vogen;

namespace Nino.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct Number
{
    public bool IsDecimal(out decimal value)
    {
        value = decimal.Zero;
        var normalized = Value.Replace(',', '.');
        return normalized.Count(c => c == '.') <= 1
            && decimal.TryParse(
                normalized,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out value
            );
    }

    public bool IsInteger(out int value)
    {
        return int.TryParse(Value, out value);
    }

    private static string NormalizeInput(string input)
    {
        // Some cultures use commas for decimal points.
        // If replacing commas with periods results in a decimal number, use that.
        // Otherwise, keep the commas.

        input = input.Trim();
        var replaced = input.Replace(',', '.');
        return decimal.TryParse(replaced, CultureInfo.InvariantCulture, out var decimalValue)
            ? decimalValue.ToString(CultureInfo.InvariantCulture)
            : input;
    }

    private static Validation Validate(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && input.Length is > 0 and <= Length.Number
            ? Validation.Ok
            : Validation.Invalid("Episode numbers must follow the constraints!");
    }
}
