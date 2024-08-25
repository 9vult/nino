using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Localizer
{
    /// <summary>
    /// Definition of a localization table for Discord commands
    /// </summary>
    internal class CommandLocalization
    {
        /// <summary>
        /// Locale code ("en-US", etc)
        /// </summary>
        public required string Locale { get; set; }
        /// <summary>
        /// Mapping of command names to their localized names
        /// </summary>
        public required Dictionary<string, DiscordLocalization> Commands { get; set; }
        /// <summary>
        /// Mapping of parameter names to their localized names
        /// </summary>
        public required Dictionary<string, DiscordLocalization> Options { get; set; }
        /// <summary>
        /// Mapping of parameter choice names to their localized names
        /// </summary>
        public required Dictionary<string, DiscordLocalization> Choices { get; set; }
    }

    internal class DiscordLocalization
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}
