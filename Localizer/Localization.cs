using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace Localizer
{
    /// <summary>
    /// Definition of a localization table for strings
    /// </summary>
    internal class Localization
    {
        /// <summary>
        /// Locale code ("en-US", etc)
        /// </summary>
        public required string Locale { get; set; }
        /// <summary>
        /// Mapping of plural type names to truthy f(x)
        /// </summary>
        /// <remarks>"one" → "x = 1" ("other" → "x <> 1" implied)</remarks>
        [JsonIgnore]
        public required FrozenDictionary<string, string> PluralDefinitions { get; set; }
        /// <summary>
        /// Mapping of lookup keys to values for singular strings
        /// </summary>
        /// <remarks>Can be used by both <see cref="Localizer.T(string, string, object[])"/> and <see cref="Localizer.T(string, string, Dictionary{string, object}, string)"/>.</remarks>
        [JsonIgnore]
        public required FrozenDictionary<string, string> Singular { get; set; }
        /// <summary>
        /// Mapping of lookup keys to a map of plural strings
        /// </summary>
        /// <remarks>Can only be used by <see cref="Localizer.T(string, string, Dictionary{string, object}, string)"/>.</remarks>
        [JsonIgnore]
        public required FrozenDictionary<string, Dictionary<string, string>> Plural { get; set; }

        [JsonPropertyName("pluralDefinitions")]
        public Dictionary<string, string> SerializationPluralDefinitions
        {
            get => new(PluralDefinitions);
            set => PluralDefinitions = FrozenDictionary.ToFrozenDictionary(value);
        }

        [JsonPropertyName("singular")]
        public Dictionary<string, string> SerializationSingular
        {
            get => new(Singular);
            set => Singular = FrozenDictionary.ToFrozenDictionary(value);
        }

        [JsonPropertyName("plural")]
        public Dictionary<string, Dictionary<string, string>> SerializationPlural
        {
            get => new(Plural);
            set => Plural = FrozenDictionary.ToFrozenDictionary(value);
        }
    }
}
