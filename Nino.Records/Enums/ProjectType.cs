using Discord.Interactions;
using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    // ReSharper disable InconsistentNaming
    public enum ProjectType
    {
        TV = 0,
        Movie = 1,

        [Hide]
        BD = 2,
        OVA = 3,
        ONA = 4,
    }

    // ReSharper restore InconsistentNaming

    public static class ProjectTypeExtensions
    {
        public static string ToFriendlyString(this ProjectType type, string lng)
        {
            return type switch
            {
                ProjectType.TV => T("choice.project.type.tv", lng),
                ProjectType.Movie => T("choice.project.type.movie", lng),
                ProjectType.BD => T("choice.project.type.bd", lng),
                ProjectType.OVA => T("choice.project.type.ova", lng),
                ProjectType.ONA => T("choice.project.type.ona", lng),
                _ => type.ToString(),
            };
        }
    }
}
