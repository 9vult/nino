using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum CongaPrefixType
    {
        None = 0,
        Nickname = 1,
        Title = 2
    }

    public static class CongaPrefixTypeExtensions
    {
        public static string ToFriendlyString(this CongaPrefixType type, string lng)
        {
            return type switch
            {
                CongaPrefixType.None => T("choice.server.congaPrefixType.none", lng),
                CongaPrefixType.Nickname => T("choice.server.congaPrefixType.nickname", lng),
                CongaPrefixType.Title => T("choice.server.congaPrefixType.title", lng),
                _ => type.ToString(),
            };
        }
    }
}
