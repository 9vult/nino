namespace Nino.Utilities;

public class NumericalStringComparer : IComparer<string>
{

    public int Compare (string? x, string? y)
    {
        if (x is not null && y is not null)
            return x.CompareNumericallyTo(y);
        
        return string.Compare(x, y, StringComparison.Ordinal);
    }
}
