using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum ProgressDisplayType
    {
        Succinct = 0,
        Verbose = 1,
    }

    public enum UpdatesDisplayType
    {
        Normal = 0,
        Extended = 1,
    }

    public static class ProgressDisplayTypeExtensions
    {
        public static string ToFriendlyString(this ProgressDisplayType type, string lng)
        {
            return type switch
            {
                ProgressDisplayType.Succinct => T("choice.server.display.type.succinct", lng),
                ProgressDisplayType.Verbose => T("choice.server.display.type.verbose", lng),
                _ => type.ToString(),
            };
        }
    }

    public static class UpdatesisplayTypeExtensions
    {
        public static string ToFriendlyString(this UpdatesDisplayType type, string lng)
        {
            return type switch
            {
                UpdatesDisplayType.Normal => T("choice.server.display.type.normal", lng),
                UpdatesDisplayType.Extended => T("choice.server.display.type.extended", lng),
                _ => type.ToString(),
            };
        }
    }
}
