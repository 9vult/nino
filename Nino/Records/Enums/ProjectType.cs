using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records.Enums
{
    internal enum ProjectType
    {
        TV = 0,
        Movie = 1,
        BD = 2
    }

    internal static class ProjectTypeExtensions
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
