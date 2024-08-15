import { Client, EmbedBuilder } from "discord.js";
import { OWNER } from "../nino";
import { nonce } from "./nonce";

export const AlertError = async (client: Client, err: Error, guild: string, project: string, projectOwner: string, location: string) => {
  const message = `[${location}]: "${err.message}" from guild \`${guild}\`, project \`${project}\``;

  console.error(message);

  const embed = new EmbedBuilder()
    .setTitle(`${err.message}`)
    .setDescription(err.stack ?? 'No stacktrace')
    .setColor(0xd797ff);
  if (OWNER) await client.users.send(OWNER, { content: message, embeds: [ embed ], ...nonce() }).catch();

  await client.users.send(projectOwner, { content: message, embeds: [ embed ], ...nonce() })
    // If the DM fails (blocked??)
    .catch(async (err: Error) => {
      const failureMessage = `[AlertError]: Could not send DM to \`${projectOwner}\`: ${err.message}`;
      console.log(failureMessage);
      if (OWNER) await client.users.send(OWNER, { content: failureMessage, ...nonce() }).catch();
    });
}