using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum AtMeType
    {
        Auto,
        Conga,
        Incomplete
    }

    public static class AtMeFilterExtensions
    {
        public static string ToFriendlyString(this AtMeType type, string lng)
        {
            return type switch
            {
                AtMeType.Auto => T("choice.atMe.type.auto", lng),
                AtMeType.Conga => T("choice.atMe.type.conga", lng),
                AtMeType.Incomplete => T("choice.atMe.type.incomplete", lng),
                _ => type.ToString(),
            };
        }
    }
}
