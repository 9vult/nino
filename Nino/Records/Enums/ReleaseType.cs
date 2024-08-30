namespace Nino.Records.Enums
{
    internal enum ReleaseType
    {
        Episode,
        Volume,
        Batch,
        Custom
    }

        internal static class ReleaseTypeExtensions
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
