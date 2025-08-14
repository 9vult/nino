# Nino Fansub Tracking Bot

Nino is a bot for tracking fansub progress on Discord.

****

## Command List

See the [Full Documentation](./docs/docs.md) for more information.

## Using Nino

You can [invite Nino to your server](https://discord.com/oauth2/authorize?client_id=803068578366095430), or you can
follow the instructions below to self-host.

## Development

Pull requests are always welcome.

## Setup for Hosting and Development

- Nino uses a SQLite database. The database should be created and migrated automatically
  on application startup, but if it doesn't, you can use `dotnet ef database update` to do so manually.

- Create an `appsettings.json` file. For development, place it in `Nino/bin/Debug/net8.0`,
  or wherever the built executable is. For hosting, place it next to the executable.

`appsettings.json` schema:

```json
{
  "Configuration": {
    "DiscordApiToken": "",
    "OwnerId": 1234
  }
}
```

## Localization

Please see the [Localization Docs](./docs/localization.md) for more information.

TL;DR: For text content, see [POEditor](https://poeditor.com/join/project/jScNllHwy9), for commands, make a pull
request.

### Localization Contributions

Big thanks to everyone who's contributed!

| Language        | Contributor         |
|-----------------|---------------------|
| Russian         | astiob<br/>JohnnyZB |
| Spanish (Spain) | Yoange              |

## License

Nino is licensed under the BSD 3-clause license

-----

© 2025 9volt.
