using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records
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
            switch (type)
            {
                case ProjectType.TV:
                    return "TV";
                case ProjectType.Movie:
                    return "Movie";
                case ProjectType.BD:
                    return "BD";
                default:
                    return type.ToString();
            }
        }
    }
}
