// SPDX-License-Identifier: MPL-2.0

using System.Text.RegularExpressions;

namespace Nino.Localization;

internal static partial class StringParser
{
    [GeneratedRegex(@"\{([^_]+)_([0-9]+)\}")]
    private static partial Regex Interpolation();

    /// <summary>
    /// Parse a string to get the interpolatable parts
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>List of parts</returns>
    /// <exception cref="LocalizationException">An error occured</exception>
    public static List<StringPart> Parse(string input)
    {
        List<StringPart> parts = [];
        try
        {
            var matches = Interpolation().Matches(input);
            parts.AddRange(
                matches.Select(match => new StringPart(
                    Name: match.Groups[1].Value,
                    Index: int.Parse(match.Groups[2].Value),
                    Match: match.Value
                ))
            );
        }
        catch (Exception e)
        {
            throw new LocalizationException("An error occured during processing:", e);
        }
        return parts;
    }

    /// <summary>
    /// Interpolate into the parent string
    /// </summary>
    /// <param name="input">The parent string</param>
    /// <param name="part">Part to replace</param>
    /// <param name="value">Value to replace with</param>
    /// <returns>The updated string</returns>
    public static string Interpolate(string input, StringPart part, object value)
    {
        return input.Replace(part.Match, value.ToString());
    }
}
