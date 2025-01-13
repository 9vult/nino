using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum AtMeFilter
    {
        Auto,
        Conga,
        Incomplete
    }

    public static class AtMeFilterExtensions
    {
        public static string ToFriendlyString(this AtMeFilter type, string lng)
        {
            return type switch
            {
                AtMeFilter.Auto => T("choice.atMe.type.auto", lng),
                AtMeFilter.Conga => T("choice.atMe.type.conga", lng),
                AtMeFilter.Incomplete => T("choice.atMe.type.incomplete", lng),
                _ => type.ToString(),
            };
        }
    }
}
