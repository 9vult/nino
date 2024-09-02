# Localizing Nino

Thank you for considering localizing Nino! Your efforts are appreciated!

## Localizing Commands

Localizing Nino's commands is relatively straightforward, if a bit repetitive. You will need to create a json file named `nino.[locale].json` in the `Nino/i18n/cmd` directory. I'd recommend copying `nino.en-US.json` to use as a starting point. Some things to consider:

- Discord doesn't usually translate command names, only descriptions. You're not obligated to follow this convention if you don't want to, but it's worth noting.
- Several command options, namely `project`, `episode`, and `abbreviation`, are used numerous times throughout the bot. I would strongly suggest using find+replace for these items for your sanity.
- Choice names appear both in the command localization and the string localization. Make sure to be consistent!

## Localizing Strings

String localization is a bit less straightforward. To start with, you will need to create a `[locale].json` in the `Nino/i18n/str` directory. I'd recommend using `en-US.json` as a starting point.

The strings file has 4 sections to be aware of:

1. `"locale": ""`. Make sure to fill this in with the correct locale!
2. `"pluralDefinitions": {}`. Here, you will need to define the plurality rules for your language in terms of _x_. You can find the rules for your language on the [Unicode CLDR](https://www.unicode.org/cldr/charts/45/supplemental/language_plural_rules.html) website.
    - I'd recommend sticking to the rule names in the CLDR.
    - Do not include a definition for "Other"

Here are a couple example definitions to help you get started:

```json
"locale": "en-US",
"pluralDefinitions": {
    "one": "x = 1"
}
```
```json
"locale": "ru",
"pluralDefinitions": {
    "one": "x % 10 = 1 AND x % 100 <> 11",
    "few": "(x % 10 >= 2 AND x % 10 <= 4) AND (x % 100 < 12 OR x % 100 > 14)",
    "many": "x % 10 = 0 OR (x % 10 >= 5 AND x % 10 <= 9) OR (x % 100 >= 11 AND x % 100 <= 14)"
}
```

3. `"singular": {}`. The bulk of the file, these are all the non-plural strings.
4. `"plural": {}`. Finally, the plural section. Here is where the `pluralDefinitions` come into play. Define a translation for each rule, plus an "other" rule. Following the above examples, English should have `one` and `other` rules, and Russian should have `one`, `few`, `many`, and `other` rules.

A key feature of string localization is interpolation. Interpolation is how the real values will be injected into the string. Nino's string localization template follows this format: `{{name|#}}`, where `name` is the name of the value being inserted, and `#` is the interpolation index. **Do not change these values!** You are free and encouraged to re-order the templates as needed, but do not change the interpolation name or index!

## A word of advice

The number of strings and commands can be daunting, so I'd reccommend starting with "public" values, like those for `/blame`, `/release`, `/done`, etc.

Good luck, and thanks again for considering localizing Nino into your language!
