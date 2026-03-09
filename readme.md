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

### Local Development

Configure user secrets for `Nino.Host`:

```bash
dotnet user-secrets init --project Nino.Host
dotnet user-secrets set "Discord:Token" "your-token-here" --project Nino.Host
dotnet user-secrets set "Discord:GuildId" "0123456789" --project Nino.Host
dotnet user-secrets set "Discord:OwnerId" "0123456789" --project Nino.Host
````

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

Nino is licensed under the MPL 2.0 license.

-----

© 2026 9volt.
