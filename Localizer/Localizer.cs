using MathEvaluation;
using MathEvaluation.Context;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Localizer
{
    public static class Localizer
    {
        private static readonly Dictionary<string, StringLocalization> _strLocales = [];
        private static readonly Dictionary<string, CommandLocalization> _cmdLocales = [];

        private static StringLocalization Fallback
        {
            get
            {
                if (!_strLocales.TryGetValue(Configuration.FallbackLocale, out var table))
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
            if (!_strLocales.TryGetValue(locale, out var table))
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
            Dictionary<string, string>? pluralTargets = null;

            if (!_strLocales.TryGetValue(locale, out var table))
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
        /// Get a map of locales/name localizations for a command
        /// </summary>
        /// <param name="command">Name of the command</param>
        /// <returns>Map of command name localizations</returns>
        public static ReadOnlyDictionary<string, string> GetCommandNames(string command)
        {
            return new ReadOnlyDictionary<string, string>(
                _cmdLocales.Values
                    .Where(l => l.Commands.ContainsKey(command))
                    .ToDictionary(t => t.Locale, t => t.Commands[command].Name)
            );
        }

        /// <summary>
        /// Get a map of locales/description localizations for a command
        /// </summary>
        /// <param name="command">Name of the command</param>
        /// <returns>Map of command description localizations</returns>
        public static ReadOnlyDictionary<string, string> GetCommandDescriptions(string command)
        {
            return new ReadOnlyDictionary<string, string>(
                _cmdLocales.Values
                    .Where(l => l.Commands.ContainsKey(command))
                    .ToDictionary(t => t.Locale, t => t.Commands[command].Description)
            );
        }

        /// <summary>
        /// Get a map of locales/name localizations for an option
        /// </summary>
        /// <param name="option">Name of the option</param>
        /// <returns>Map of option name localizations</returns>
        public static ReadOnlyDictionary<string, string> GetOptionNames(string option)
        {
            return new ReadOnlyDictionary<string, string>(
                _cmdLocales.Values
                    .Where(l => l.Options.ContainsKey(option))
                    .ToDictionary(t => t.Locale, t => t.Options[option].Name)
            );
        }

        /// <summary>
        /// Get a map of locales/description localizations for an option
        /// </summary>
        /// <param name="option">Name of the option</param>
        /// <returns>Map of option description localizations</returns>
        public static ReadOnlyDictionary<string, string> GetOptionDescriptions(string option)
        {
            return new ReadOnlyDictionary<string, string>(
                _cmdLocales.Values
                    .Where(l => l.Options.ContainsKey(option))
                    .ToDictionary(t => t.Locale, t => t.Options[option].Description)
            );
        }

        /// <summary>
        /// Get a map of locales/name localizations for a choice
        /// </summary>
        /// <param name="choice">Name of the choice</param>
        /// <returns>Map of choice name localizations</returns>
        public static ReadOnlyDictionary<string, string> GetChoiceNames(string choice)
        {
            return new ReadOnlyDictionary<string, string>(
                _cmdLocales.Values
                    .Where(l => l.Choices.ContainsKey(choice))
                    .ToDictionary(t => t.Locale, t => t.Choices[choice].Description)
            );
        }

        /// <summary>
        /// Load string localization files from the directory specified
        /// </summary>
        /// <param name="directory">Uri to the directory</param>
        public static void LoadStringLocalizations(Uri directory)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            try
            {
                foreach (var file in Directory.EnumerateFiles(directory.LocalPath, "*.json"))
                {
                    try
                    {
                        using StreamReader sr = new(file);
                        var table = JsonSerializer.Deserialize<StringLocalization>(sr.ReadToEnd(), options);
                        if (table == null) continue;
                        _strLocales.Add(table.Locale, table);
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
        }

        /// <summary>
        /// Load command localization files from the directory specified
        /// </summary>
        /// <param name="directory">Uri to the directory</param>
        public static void LoadCommandLocalizations(Uri directory)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            try
            {
                foreach (var file in Directory.EnumerateFiles(directory.LocalPath, "*.json"))
                {
                    try
                    {
                        using StreamReader sr = new(file);
                        var table = JsonSerializer.Deserialize<CommandLocalization>(sr.ReadToEnd(), options);
                        if (table == null) continue;
                        _cmdLocales.Add(table.Locale, table);
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
        private static string ResolvePluralTarget(decimal value, Dictionary<string, string> definitions, Dictionary<string, string> targets)
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
