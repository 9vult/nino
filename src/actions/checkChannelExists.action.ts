import { Client } from "discord.js";

export const CheckChannelExists = (client: Client, channelId: string) => {
  const channel = client.channels.cache.get(channelId);
  if (channel) return true;
  return false;
}
