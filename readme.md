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

Localization pull requests are welcome! Please use `i18n/en_us.json` as your template, and don't break the template strings :D (`$VARIABLE`)

### Development

Pull requests are always welcome.

### License

Nino is licensed under LGPL v3.0.


© 2024 9volt.
