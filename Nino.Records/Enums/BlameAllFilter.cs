using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum BlameAllFilter
    {
        All,
        InProgress,
        Incomplete
    }

    public static class BlameAllFilterExtensions
    {
        public static string ToFriendlyString(this BlameAllFilter type, string lng)
        {
            return type switch
            {
                BlameAllFilter.All => T("choice.blameall.type.all", lng),
                BlameAllFilter.InProgress => T("choice.blameall.type.inProgress", lng),
                BlameAllFilter.Incomplete => T("choice.blameall.type.incomplete", lng),
                _ => type.ToString(),
            };
        }
    }
}
