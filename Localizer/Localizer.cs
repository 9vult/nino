using MathEvaluation;
using MathEvaluation.Context;
using System.Collections.Frozen;
using System.Text.Json;

namespace Localizer
{
    public static class Localizer
    {
        private static FrozenDictionary<string, Localization>? _locales;

        private static Localization Fallback
        {
            get
            {
                if (_locales is null) throw new LocalizationException($"Locales were not set up!");
                if (!_locales.TryGetValue(Configuration.FallbackLocale, out var table))
                    throw new LocalizationException($"Fallback locale {Configuration.FallbackLocale} was not found.");
                return table;
            }
        }

        /// <summary>
        /// Localize a string using indexes. Does *not* support Plurals
        /// </summary>
        /// <param name="key">String lookup key</param>
        /// <param name="locale">Locale to localize to</param>
        /// <param name="args">Positional inputs</param>
        /// <returns>Localized string</returns>
        /// <exception cref="LocalizationException">Key was not found</exception>
        public static string T(string key, string locale, params object[] args)
        {
            if (_locales is null) throw new LocalizationException($"Locales were not set up!");
            if (!_locales.TryGetValue(locale, out var table))
                table = Fallback;

            if (!table.Singular.TryGetValue(key, out var target))
                if (!Fallback.Singular.TryGetValue(key, out target))
                    throw new LocalizationException($"No suitable singular match found for {key}.");
            if (target == null) throw new LocalizationException($"No suitable singular match found for {key}.");

            var parts = StringParser.Parse(target);

            if (args.Length == 0) return target;
            foreach (var part in parts)
            {
                int index = part.Index - 1; // parts are 1-indexed
                if (index < 0 || index > args.Length) continue;

                target = StringParser.Interpolate(target, part, args[index]);
            }
            return target;
        }

        /// <summary>
        /// Localize a string using indexes. *Does* support Plurals
        /// </summary>
        /// <param name="key">String lookup key</param>
        /// <param name="locale">Locale to localize to</param>
        /// <param name="args">Named inputs</param>
        /// <param name="pluralName">Optional override name of the plural</param>
        /// <returns>Localized string</returns>
        /// <exception cref="LocalizationException">Key was not found</exception>
        public static string T(string key, string locale, Dictionary<string, object> args, string pluralName = "number")
        {
            if (_locales is null) throw new LocalizationException($"Locales were not set up!");
            Dictionary<string, string>? pluralTargets = null;

            if (!_locales.TryGetValue(locale, out var table))
                table = Fallback;

            if (!table.Singular.TryGetValue(key, out var target))
                if (!Fallback.Singular.TryGetValue(key, out target))
                    if (!table.Plural.TryGetValue(key, out pluralTargets))
                        if (!Fallback.Plural.TryGetValue(key, out pluralTargets))
                            throw new LocalizationException($"No suitable singular or plural match found for {key}.");

            // Resolve plurals if applicable
            if (pluralTargets != null)
            {
                if (!args.TryGetValue(pluralName, out var pluralObj))
                    throw new LocalizationException($"No match found for plural {pluralName}");
                decimal pluralValue = Convert.ToDecimal(pluralObj);
                target = ResolvePluralTarget(pluralValue, table.PluralDefinitions, pluralTargets);
            }

            if (target == null) throw new LocalizationException($"No suitable singular or plural match found for {key}.");

            var parts = StringParser.Parse(target);
            foreach (var part in parts)
            {
                string name = part.Name;
                if (args.TryGetValue(name, out var arg))
                {
                    target = StringParser.Interpolate(target, part, arg);
                }
            }
            return target;
        }

        /// <summary>
        /// Load localization files from the directory specified
        /// </summary>
        /// <param name="directory">Uri to the directory</param>
        public static void LoadLocalizations(Uri directory)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var locales = new Dictionary<string, Localization>();

            try
            {
                foreach (var file in Directory.EnumerateFiles(directory.LocalPath, "*.json"))
                {
                    try
                    {
                        using StreamReader sr = new(file);
                        var table = JsonSerializer.Deserialize<Localization>(sr.ReadToEnd(), options);
                        if (table == null) continue;
                        locales.Add(table.Locale, table);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            _locales = FrozenDictionary.ToFrozenDictionary(locales);
        }

        /// <summary>
        /// Resolve the correct plural form to use
        /// </summary>
        /// <param name="value">Decimal value</param>
        /// <param name="definitions">Plural definitions for the locale</param>
        /// <param name="targets">Potential plural forms</param>
        /// <returns>Target matching the correct plural form</returns>
        /// <exception cref="LocalizationException"></exception>
        /// <exception cref="FormatException">An equation was malformed</exception>
        /// <exception cref="NotSupportedException">An equation did something not supported</exception>
        private static string ResolvePluralTarget(decimal value, FrozenDictionary<string, string> definitions, Dictionary<string, string> targets)
        {
            var context = new ProgrammingMathContext();
            string? pluralType = null;
            foreach (var definition in definitions)
            {
                bool match = definition.Value
                    .SetContext(context)
                    .BindVariable(value, "x")
                    .EvaluateBoolean();

                if (!match) continue;
                pluralType = definition.Key;
            }

            pluralType ??= "other";

            if (!targets.TryGetValue(pluralType, out var target))
                throw new LocalizationException($"Plural type {pluralType} was not found.");
            return target;
        }
    }
}
