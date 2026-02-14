// SPDX-License-Identifier: MPL-2.0

using System.Collections.Frozen;
using System.Globalization;
using System.Text.Json;
using ICU4N.Text;

namespace Nino.Localization;

public static class Localizer
{
    private static FrozenDictionary<string, Localization>? _locales;
    private static readonly Dictionary<string, CultureInfo> Cultures = new();

    /// <summary>
    /// Get a culture info
    /// </summary>
    /// <param name="discordLocale">Discord locale code</param>
    /// <returns>Culture info for the given locale, or the fallback locale</returns>
    /// <exception cref="LocalizationException">Locale and fallback were not found</exception>
    public static CultureInfo GetCultureInfo(string discordLocale)
    {
        var dotNetLocale = discordLocale.FromDiscordLocale().ToDotNetLocale(); // Convert Discord naming to .NET naming
        if (Cultures.TryGetValue(dotNetLocale, out var cultureInfo))
            return cultureInfo;
        if (Cultures.TryGetValue(Configuration.FallbackLocale, out cultureInfo))
            return cultureInfo;
        throw new LocalizationException(
            $"Fallback locale {Configuration.FallbackLocale} was not found."
        );
    }

    private static Localization Fallback
    {
        get
        {
            if (_locales is null)
                throw new LocalizationException("Locales were not set up!");
            if (!_locales.TryGetValue(Configuration.FallbackLocale, out var table))
                throw new LocalizationException(
                    $"Fallback locale {Configuration.FallbackLocale} was not found."
                );
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
        if (_locales is null)
            throw new LocalizationException("Locales were not set up!");
        if (!_locales.TryGetValue(locale, out var table))
            table = Fallback;

        if (!table.Singular.TryGetValue(key, out var target))
            if (!Fallback.Singular.TryGetValue(key, out target))
                throw new LocalizationException($"No suitable singular match found for {key}.");
        if (target == null)
            throw new LocalizationException($"No suitable singular match found for {key}.");

        var parts = StringParser.Parse(target);

        if (args.Length == 0)
            return target;
        foreach (var part in parts)
        {
            var index = part.Index - 1; // parts are 1-indexed
            if (index < 0 || index > args.Length)
                continue;

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
    public static string T(
        string key,
        string locale,
        Dictionary<string, object> args,
        string pluralName = "number"
    )
    {
        if (_locales is null)
            throw new LocalizationException("Locales were not set up!");
        Dictionary<string, string>? pluralTargets = null;

        if (!_locales.TryGetValue(locale, out var table))
            table = Fallback;

        if (!table.Singular.TryGetValue(key, out var target))
            if (!Fallback.Singular.TryGetValue(key, out target))
                if (!table.Plural.TryGetValue(key, out pluralTargets))
                    if (!Fallback.Plural.TryGetValue(key, out pluralTargets))
                        throw new LocalizationException(
                            $"No suitable singular or plural match found for {key}."
                        );

        // Resolve plurals if applicable
        if (pluralTargets != null)
        {
            if (!args.TryGetValue(pluralName, out var pluralObj))
                throw new LocalizationException($"No match found for plural {pluralName}");
            var pluralValue = Convert.ToDouble(pluralObj);
            target = ResolvePluralTarget(pluralValue, table.PluralRules, pluralTargets);
        }

        if (target == null)
            throw new LocalizationException(
                $"No suitable singular or plural match found for {key}."
            );

        var parts = StringParser.Parse(target);
        foreach (var part in parts)
        {
            var name = part.Name;
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
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var locales = new Dictionary<string, Localization>();

        try
        {
            foreach (var file in Directory.EnumerateFiles(directory.LocalPath, "*.json"))
            {
                try
                {
                    using StreamReader sr = new(file);
                    var table = JsonSerializer.Deserialize<Localization>(sr.ReadToEnd(), options);
                    if (table == null)
                        continue;

                    var locale = Path.GetFileNameWithoutExtension(file);
                    var ci = new CultureInfo(locale);
                    Cultures.Add(locale.FromDiscordLocale().ToDotNetLocale(), ci); // Convert Discord naming to .NET naming
                    table.PluralRules = PluralRules.GetInstance(ci);

                    locales.Add(locale, table);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        _locales = locales.ToFrozenDictionary();
    }

    /// <summary>
    /// Resolve the correct plural form to use
    /// </summary>
    /// <param name="value">Double value</param>
    /// <param name="rules">Plural rules to use</param>
    /// <param name="targets">Potential plural forms</param>
    /// <returns>Target matching the correct plural form</returns>
    /// <exception cref="LocalizationException">Plural of type was not found</exception>
    private static string ResolvePluralTarget(
        double value,
        PluralRules? rules,
        Dictionary<string, string> targets
    )
    {
        var pluralType = rules?.Select(value) ?? "other";

        if (!targets.TryGetValue(pluralType, out var target))
            throw new LocalizationException($"Plural type {pluralType} was not found.");
        return target;
    }
}
