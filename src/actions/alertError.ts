import { Client, EmbedBuilder } from "discord.js";
import { OWNER } from "../nino";

export const AlertError = async (client: Client, err: Error, guild: string, project: string, location: string) => {
  const message = `[${location}]: "${err.message}" from guild \`${guild}\`, project \`${project}\``;

  console.error(message);

  const embed = new EmbedBuilder()
    .setTitle(`${err.message}`)
    .setDescription(err.stack ?? 'No stacktrace')
    .setColor(0xd797ff);
  if (OWNER) await client.users.send(OWNER, { content: message, embeds: [ embed ] });
}