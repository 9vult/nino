using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum ProjectType
    {
        TV = 0,
        Movie = 1,
        BD = 2
    }

    public static class ProjectTypeExtensions
    {
        public static string ToFriendlyString(this ProjectType type, string lng)
        {
            return type switch
            {
                ProjectType.TV => T("choice.project.type.tv", lng),
                ProjectType.Movie => T("choice.project.type.movie", lng),
                ProjectType.BD => T("choice.project.type.bd", lng),
                _ => type.ToString(),
            };
        }
    }
}
