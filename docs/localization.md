# Localizing Nino

Thank you for considering localizing Nino! Your efforts are appreciated!

## Localizing Commands

Localizing Nino's commands is relatively straightforward, if a bit repetitive. You will need to create a json file named `nino.[locale].json` in the `Nino/i18n/cmd` directory. I'd recommend copying `nino.en-US.json` to use as a starting point. Some things to consider:

- Discord doesn't usually translate command names, only descriptions. You're not obligated to follow this convention if you don't want to, but it's worth noting.
- Several command options, namely `project`, `episode`, and `abbreviation`, are used numerous times throughout the bot. I would strongly suggest using find+replace for these items for your sanity.
- Choice names appear both in the command localization and the string localization. Make sure to be consistent!

Finally, make a pull request!

## Localizing Strings

String localization is now super straightforward! Visit the project on [POEditor](https://poeditor.com/join/project/jScNllHwy9) and start translating!

A key feature of string localization is interpolation. Interpolation is how the real values will be injected into the string. Nino's string localization template follows this format: `{name_#}`, where `name` is the name of the value being inserted, and `#` is the interpolation index. **Do not change these values!** You are free and encouraged to re-order the templates as needed, but do not change the interpolation name or index!

## A word of advice

The number of strings and commands can be daunting, so I'd recommend starting with "public" values, like those for `/blame`, `/release`, `/done`, etc.

Good luck, and thanks again for considering localizing Nino into your language!
