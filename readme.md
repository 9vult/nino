# Nino Fansub Tracking Bot

Nino is a bot for tracking fansub progress on Discord.

****

### Setup

 - Nino requires a [Firebase Real-Time Database](https://firebase.google.com/docs/database) for logging and leaderboards. The base tier is free, and it is highly unlikely Nino will ever generate enough data to exceed the base tier.

Create a `.env` file in the project root and add the following to it: 

 - `TOKEN=[yourtoken]`
 - `DATABASE_URL=[databaseurl]`

Then, place your `firebase.json` in the `/src/` folder.

### Command List

- /help
- /about
- /newproject
- /editproject
- /deleteproject
- /addstaff
- /swapstaff
- /removestaff
- /addadditionalstaff
- /swapadditionalstaff
- /removeadditionalstaff
- /transferownership
- /addepisode
- /removeepisode
- /done
- /undone
- /release

### Development

Pull requests are always welcome.

### License

Nino is licensed under LGPL v3.0.


© 2024 9volt.
