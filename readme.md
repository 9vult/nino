# Nino Fansub Tracking Bot

Nino is a bot for tracking fansub progress on Discord. See the [documentation](./docs/docs.md) for details and
instructions.

****

## Using Nino

You can [invite Nino to your server](https://discord.com/oauth2/authorize?client_id=803068578366095430), or you can
follow the instructions below to self-host.

## Setup for Hosting or Development

- Nino uses a SQLite database. The database should be created and migrated automatically
  on application startup, but if it doesn't, you can use `dotnet ef database update` to do so manually.

- Configure the `Discord` section in `appsettings.json` or use `dotnet secrets` for development.

## License

Nino is licensed under the MPL 2.0 license

-----

© 2026 9volt.
