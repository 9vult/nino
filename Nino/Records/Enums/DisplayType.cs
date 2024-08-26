using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records.Enums
{
    internal enum DisplayType
    {
        Normal = 0,
        Extended = 1,
        Succinct = 2,
        Verbose = 3
    }

    internal static class ProgressDisplayExtensions
    {
        public static string ToFriendlyString(this DisplayType type)
        {
            return type switch
            {
                DisplayType.Normal => "Normal",
                DisplayType.Extended => "Extended",
                DisplayType.Succinct => "Succinct",
                DisplayType.Verbose => "Verbose",
                _ => type.ToString(),
            };
        }
    }
}
