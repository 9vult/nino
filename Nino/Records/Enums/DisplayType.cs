namespace Nino.Records.Enums
{
    public enum ProgressDisplayType
    {
        Succinct = 0,
        Verbose = 1
    }

    public enum UpdatesDisplayType
    {
        Normal = 0,
        Extended = 1
    }

    public static class ProgressDisplayTypeExtensions
    {
        public static string ToFriendlyString(this ProgressDisplayType type)
        {
            return type switch
            {
                ProgressDisplayType.Succinct => "Succinct",
                ProgressDisplayType.Verbose => "Verbose",
                _ => type.ToString(),
            };
        }
    }

        public static class UpdatesisplayTypeExtensions
    {
        public static string ToFriendlyString(this UpdatesDisplayType type)
        {
            return type switch
            {
                UpdatesDisplayType.Normal => "Normal",
                UpdatesDisplayType.Extended => "Extended",
                _ => type.ToString(),
            };
        }
    }
}
