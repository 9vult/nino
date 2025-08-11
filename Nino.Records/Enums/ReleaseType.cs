using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum ReleaseType
    {
        Episode = 0,
        Volume = 1,
        Batch = 2,
        Custom = 3,
    }

    public static class ReleaseTypeExtensions
    {
        public static string ToFriendlyString(this ReleaseType type, string lng)
        {
            return type switch
            {
                ReleaseType.Episode => T("choice.release.type.episode", lng),
                ReleaseType.Volume => T("choice.release.type.volume", lng),
                ReleaseType.Batch => T("choice.release.type.batch", lng),
                ReleaseType.Custom => T("choice.release.type.custom", lng),
                _ => type.ToString(),
            };
        }
    }
}
