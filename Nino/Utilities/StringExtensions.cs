namespace Nino.Utilities;

public static class StringExtensions
{
    /// <summary>
    /// Compares two strings numerically if possible; otherwise, lexicographically.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>
    /// A value less than zero if x is less than y.
    /// Zero if x equals y.
    /// A value greater than zero if x is greater than y.
    /// </returns>
    public static int CompareNumericallyTo(this string x, string y)
    {
        // Attempt to parse both strings as integers
        var isXNumeric = decimal.TryParse(x, out var xNum);
        var isYNumeric = decimal.TryParse(y, out var yNum);

        // Case 1: Both strings are numeric
        if (isXNumeric && isYNumeric) return xNum.CompareTo(yNum);

        // Case 2: Only one string is numeric
        if (isXNumeric) return -1; // Numbers come before non-numbers
        if (isYNumeric) return 1;  // Non-numbers come after numbers

        // Case 3: Both strings are non-numeric, compare lexicographically
        return string.Compare(x, y, StringComparison.Ordinal);
    }
}
