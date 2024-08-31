
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
        public static string ToFriendlyString(this ProjectType type)
        {
            return type switch
            {
                ProjectType.TV => "TV",
                ProjectType.Movie => "Movie",
                ProjectType.BD => "BD",
                _ => type.ToString(),
            };
        }
    }
}
