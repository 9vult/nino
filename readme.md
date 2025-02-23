# Nino Fansub Tracking Bot

Nino is a bot for tracking fansub progress on Discord.

****

## Command List

See the [Full Documentation](./docs/docs.md) for more information.

## Development

Pull requests are always welcome.

## Setup for Hosting and Development

 - Nino requires an [Azure Cosmos DB](https://azure.microsoft.com/en-us/products/cosmos-db). The base tier is free, and it is highly unlikely Nino will ever generate enough data to exceed the base tier.

- Create an `appsettings.json` file. For development, place it in `Nino/bin/Debug/net8.0`,
or wherever the built executable is. For hosting, place it next to the executable.

`appsettings.json` schema:

```json
{
    "Configuration": {
        "AzureCosmosEndpoint": "",
        "AzureClientSecret": "",
        "AzureCosmosDbName": "",
        "DiscordApiToken": "",
        "OwnerId": 1234
    }
}
```

## Localization

Localization pull requests are more than welcome! Please see the [Localization Docs](./docs/localization.md) for more information.

<img src="https://docs.google.com/spreadsheets/d/e/2PACX-1vSE6onkEKAwG4d2vT44LxWv0jmYfBUKln_Oi3uzYB47IS7Bf9E4bqCc6-Wyc__0QeLbTiGYdvkvonRo/pubchart?oid=1917133197&format=image" alt="Localization progress by language" width="75%">

## License

Nino is licensed under the BSD 3-clause license

-----

© 2024 9volt.
