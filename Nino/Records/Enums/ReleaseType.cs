namespace Nino.Records.Enums
{
    public enum ReleaseType
    {
        Episode,
        Volume,
        Batch,
        Custom
    }

    public static class ReleaseTypeExtensions
    {
        public static string ToFriendlyString(this ReleaseType type)
        {
            return type switch
            {
                ReleaseType.Episode => "Episode",
                ReleaseType.Volume => "Volume",
                ReleaseType.Batch => "Batch",
                ReleaseType.Custom => "Custom",
                _ => type.ToString(),
            };
        }
    }
}
