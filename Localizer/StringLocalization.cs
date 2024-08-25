using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Localizer
{
    /// <summary>
    /// Definition of a localization table for strings
    /// </summary>
    internal class StringLocalization
    {
        /// <summary>
        /// Locale code ("en-US", etc)
        /// </summary>
        public required string Locale { get; set; }
        /// <summary>
        /// Mapping of plural type names to truthy f(x)
        /// </summary>
        /// <remarks>"one" → "x = 1" ("other" → "x <> 1" implied)</remarks>
        public required Dictionary<string, string> PluralDefinitions { get; set; }
        /// <summary>
        /// Mapping of lookup keys to values for singular strings
        /// </summary>
        /// <remarks>Can be used by both <see cref="Localizer.T(string, string, object[])"/> and <see cref="Localizer.T(string, string, Dictionary{string, object}, string)"/>.</remarks>
        public required Dictionary<string, string> Singular { get; set; }
        /// <summary>
        /// Mapping of lookup keys to a map of plural strings
        /// </summary>
        /// <remarks>Can only be used by <see cref="Localizer.T(string, string, Dictionary{string, object}, string)"/>.</remarks>
        public required Dictionary<string, Dictionary<string, string>> Plural { get; set; }
    }
}
