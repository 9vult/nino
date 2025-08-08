namespace Nino.Utilities.Extensions;

public static class DateTimeExtensions
{
    public static double ToUnixTimeSeconds(this DateTime date)
    {
        var diff = date.ToUniversalTime() - DateTime.UnixEpoch;
        return Math.Floor(diff.TotalSeconds);
    }
}