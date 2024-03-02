# Raiha Accessibility Bot

Raiha is an alt text helper bot that enables adding alt text to new or existing messages.

 - To create a new message with Raiha, write a message as normal, then add the Raiha trigger command to the end of the message. For example, `This is a picture of my cat! r! Tabby kitten sitting in a sink`
 - To add alt text to an existing message, reply to the original message with a Raiha trigger command: `r! Alt text for the image`.
 - If there is more than one image in the message, split the alt text with `|`: `r! Image 1 | Image 2 | Image 3`.
 - Raiha supports the following triggers: `r!`, `alt:`, `id:`
 - Raiha supports Azure Computer Vision. Use `r! $$` to generate an image description, and `r! $$ocr` to add OCR. These apply per-image, and can be mixed and matched: `r! my cat | $$ | my dog | $$ocr`

****
### For moderation

Raiha will post a message to a channel of your choosing to alert moderators when a user's Loserboard score surpasses a multiple of your choice, as well as reminding them if a user is 5 strikes from the threshold.


### Setup

 - Raiha requires a [Firebase Real-Time Database](https://firebase.google.com/docs/database) for logging and leaderboards. The base tier is free, and it is highly unlikely Raiha will ever generate enough data to exceed the base tier.
 - Raiha also requires an [Azure Cognitive Vision Service](https://learn.microsoft.com/en-us/azure/cognitive-services/custom-vision-service/limits-and-quotas) instance. The base tier is free, and allows for 20 requests per minute, 5000 per month.

Create a `.env` file in the project root and add the following to it: 

 - `TOKEN=[yourtoken]`
 - `DATABASE_URL=[databaseurl]`
 - `MOD_CHANNEL=[channelid]`
 - `CV_API_KEY=[yourkey]`
 - `CV_ENDPOINT=[endpoint, with trailing /]`

Then, place your `firebase.json` in the `/src/` folder.

Finally, in the firebase database, set the server configuration at `/Configuration/[guildID]`:

```typescript
{
  ai: boolean,
  altrules: "default" | string,
  enableWarnings: boolean,
  errorChannel: string (channelID),
  errorMismatch: "default" | string (emojiID),
  errorNoAlt: "default" | string (emojiID),
  errorNotReply: "default" | string (emojiID),
  leaderboard: boolean,
  loserboard: boolean,
  modChannel: string (channelID),
  modRole: string (roleName),
  muteThreshold: number (0 to disable),
  specialWarnThresholds: number[] (ignores enableWarnings value)
}
```

_Some of these options are not yet implemented. Data types and names may change._

### Development

Pull requests are always welcome.

### License

Raiha is licensed under LGPL v3.0.


© 2022 9volt.
