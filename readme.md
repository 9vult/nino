# Nino Fansub Tracking Bot

Nino is a bot for tracking fansub progress on Discord.

****

### Command List

See the [Documentation](./docs.md) for more information.

### Localization

Localization pull requests are welcome! You will need to create two json files, one in `i18n/cmd` for slash commands, and one in `i18n/str` for strings. For plurals, see the information on the [i18next website](https://www.i18next.com/translation-function/plurals). 

### Development

Pull requests are always welcome.

### Setup for Hosting or Development

 - Nino requires a [Firebase Real-Time Database](https://firebase.google.com/docs/database). The base tier is free, and it is highly unlikely Nino will ever generate enough data to exceed the base tier.

Create a `.env` file in the project root and add the following to it: 

 - `TOKEN=[yourtoken]`
 - `DATABASE_URL=[databaseurl]`
 - `ANIDB_API_CLIENT_NAME=[yourclientname]`
 - `OWNER=[yourdiscorduserid]`

Then, place your `firebase.json` in the `/src/` folder.

### License

Nino is licensed under LGPL v3.0.


© 2024 9volt.
