using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum BlameAllType
    {
        Normal,
        StallCheck,
    }

    public static class BlameAllTypeExtensions
    {
        public static string ToFriendlyString(this BlameAllType type, string lng)
        {
            return type switch
            {
                BlameAllType.Normal => T("choice.blameall.type.normal", lng),
                BlameAllType.StallCheck => T("choice.blameall.type.stallcheck", lng),
                _ => type.ToString(),
            };
        }
    }
}
