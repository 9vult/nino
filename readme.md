# Nino Fansub Tracking Bot

Nino is a bot for tracking fansub progress on Discord.

****

### Setup

 - Nino requires a [Firebase Real-Time Database](https://firebase.google.com/docs/database) for logging and leaderboards. The base tier is free, and it is highly unlikely Nino will ever generate enough data to exceed the base tier.

Create a `.env` file in the project root and add the following to it: 

 - `TOKEN=[yourtoken]`
 - `DATABASE_URL=[databaseurl]`
 - `ANIDB_API_CLIENT_NAME=[yourclientname]`

Then, place your `firebase.json` in the `/src/` folder.

### Command List

See the [Documentation](./docs.md) for more information.

### Localization

Localization pull requests are welcome! You will need to create two json files, one in `i18n/cmd` for slash commands, and one in `i18n/str` for strings. For plurals, see the information on the [i18next website](https://www.i18next.com/translation-function/plurals). 

### Development

Pull requests are always welcome.

### License

Nino is licensed under LGPL v3.0.


© 2024 9volt.
