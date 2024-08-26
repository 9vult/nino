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
            switch (type)
            {
                case DisplayType.Normal:
                    return "Normal";
                case DisplayType.Extended:
                    return "Extended";
                case DisplayType.Succinct:
                    return "Succinct";
                case DisplayType.Verbose:
                    return "Verbose";
                default:
                    return type.ToString();
            }
        }
    }
}
