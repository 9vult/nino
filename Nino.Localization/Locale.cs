// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;

namespace Nino.Localization;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum Locale
{
    EnglishUS,
    EnglishGB,
    Indonesian,
    Bulgarian,
    ChineseCN,
    ChineseTW,
    Croatian,
    Czech,
    Danish,
    Dutch,
    Finnish,
    French,
    German,
    Greek,
    Hindi,
    Hungarian,
    Italian,
    Japanese,
    Korean,
    Lithuanian,
    Norwegian,
    Polish,
    PortugueseBR,
    Romanian,
    Russian,
    SpanishES,
    Spanish419,
    Swedish,
    Thai,
    Turkish,
    Ukrainian,
    Vietnamese,
}

public static class LocaleExtensions
{
    public static string ToDiscordLocale(this Locale type)
    {
        return type switch
        {
            Locale.EnglishUS => "en-US",
            Locale.EnglishGB => "en-GB",
            Locale.Indonesian => "id",
            Locale.Bulgarian => "bg",
            Locale.ChineseCN => "zh-CN",
            Locale.ChineseTW => "zh-TW",
            Locale.Croatian => "hr",
            Locale.Czech => "cs",
            Locale.Danish => "da",
            Locale.Dutch => "nl",
            Locale.Finnish => "fi",
            Locale.French => "fr",
            Locale.German => "de",
            Locale.Greek => "el",
            Locale.Hindi => "hi",
            Locale.Hungarian => "hu",
            Locale.Italian => "it",
            Locale.Japanese => "ja",
            Locale.Korean => "ko",
            Locale.Lithuanian => "lt",
            Locale.Norwegian => "no",
            Locale.Polish => "pl",
            Locale.PortugueseBR => "pt-BR",
            Locale.Romanian => "ro",
            Locale.Russian => "ru",
            Locale.SpanishES => "es-ES",
            Locale.Spanish419 => "es-419",
            Locale.Swedish => "sv-SE",
            Locale.Thai => "th",
            Locale.Turkish => "tr",
            Locale.Ukrainian => "uk",
            Locale.Vietnamese => "vi",
            _ => type.ToString(),
        };
    }

    public static string ToDotNetLocale(this Locale type)
    {
        return type switch
        {
            Locale.EnglishUS => "en-US",
            Locale.EnglishGB => "en-GB",
            Locale.Indonesian => "id-ID",
            Locale.Bulgarian => "bg-BG",
            Locale.ChineseCN => "zh-Hans-CN",
            Locale.ChineseTW => "zh-Hant-TW",
            Locale.Croatian => "hr-HR",
            Locale.Czech => "cs-CZ",
            Locale.Danish => "da-DK",
            Locale.Dutch => "nl-NL",
            Locale.Finnish => "fi-FI",
            Locale.French => "fr-FR",
            Locale.German => "de-DE",
            Locale.Greek => "el-GR",
            Locale.Hindi => "hi-IN",
            Locale.Hungarian => "hu-HU",
            Locale.Italian => "it-IT",
            Locale.Japanese => "ja-JP",
            Locale.Korean => "ko-KR",
            Locale.Lithuanian => "lt-LT",
            Locale.Norwegian => "nn-NO",
            Locale.Polish => "pl-PL",
            Locale.PortugueseBR => "pt-BR",
            Locale.Romanian => "ro-RO",
            Locale.Russian => "ru-RU",
            Locale.SpanishES => "es-ES",
            Locale.Spanish419 => "es-419",
            Locale.Swedish => "sv-SE",
            Locale.Thai => "th-TH",
            Locale.Turkish => "tr-TR",
            Locale.Ukrainian => "uk-UA",
            Locale.Vietnamese => "vi-VN",
            _ => type.ToString(),
        };
    }

    public static string ToFriendlyString(this Locale type)
    {
        return type switch
        {
            Locale.EnglishUS => "English (US)",
            Locale.EnglishGB => "English (UK)",
            Locale.Indonesian => "Indonesian",
            Locale.Bulgarian => "Bulgarian",
            Locale.ChineseCN => "Chinese (China)",
            Locale.ChineseTW => "Chinese (Taiwan)",
            Locale.Croatian => "Croatian",
            Locale.Czech => "Czech",
            Locale.Danish => "Danish",
            Locale.Dutch => "Dutch",
            Locale.Finnish => "Finnish",
            Locale.French => "French",
            Locale.German => "German",
            Locale.Greek => "Greek",
            Locale.Hindi => "Hindi",
            Locale.Hungarian => "Hungarian",
            Locale.Italian => "Italian",
            Locale.Japanese => "Japanese",
            Locale.Korean => "Korean",
            Locale.Lithuanian => "Lithuanian",
            Locale.Norwegian => "Norwegian",
            Locale.Polish => "Polish",
            Locale.PortugueseBR => "Portuguese (Brazil)",
            Locale.Romanian => "Romanian",
            Locale.Russian => "Russian",
            Locale.SpanishES => "Spanish (Spain)",
            Locale.Spanish419 => "Spanish (LATAM)",
            Locale.Swedish => "Swedish",
            Locale.Thai => "Thai",
            Locale.Turkish => "Turkish",
            Locale.Ukrainian => "Ukrainian",
            Locale.Vietnamese => "Vietnamese",
            _ => type.ToString(),
        };
    }

    public static Locale FromDiscordLocale(this string discordLocale)
    {
        return discordLocale switch
        {
            "en-US" => Locale.EnglishUS,
            "en-GB" => Locale.EnglishGB,
            "id" => Locale.Indonesian,
            "bg" => Locale.Bulgarian,
            "zh-CN" => Locale.ChineseCN,
            "zh-TW" => Locale.ChineseTW,
            "hr" => Locale.Croatian,
            "cs" => Locale.Czech,
            "da" => Locale.Danish,
            "nl" => Locale.Dutch,
            "fi" => Locale.Finnish,
            "fr" => Locale.French,
            "de" => Locale.German,
            "el" => Locale.Greek,
            "hi" => Locale.Hindi,
            "hu" => Locale.Hungarian,
            "it" => Locale.Italian,
            "ja" => Locale.Japanese,
            "ko" => Locale.Korean,
            "lt" => Locale.Lithuanian,
            "no" => Locale.Norwegian,
            "pl" => Locale.Polish,
            "pt-BR" => Locale.PortugueseBR,
            "ro" => Locale.Romanian,
            "ru" => Locale.Russian,
            "es-ES" => Locale.SpanishES,
            "es-419" => Locale.Spanish419,
            "sv-SE" => Locale.Swedish,
            "th" => Locale.Thai,
            "tr" => Locale.Turkish,
            "uk" => Locale.Ukrainian,
            "vi" => Locale.Vietnamese,
            _ => throw new ArgumentException($"Unknown locale: {discordLocale}"),
        };
    }
}
