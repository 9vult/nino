import { Client, TextChannel } from "discord.js";

export const CheckChannelPerms = (client: Client, channelId: string, releaseChannel = false) => {
  let channel = client.channels.cache.get(channelId);
  if (!channel) return false;
  channel = (channel as TextChannel)

  if (releaseChannel)
    return channel.permissionsFor(client.user!.id)?.has([ "ViewChannel", "SendMessages", "MentionEveryone", "EmbedLinks" ]);
  else
    return channel.permissionsFor(client.user!.id)?.has([ "ViewChannel", "SendMessages", "EmbedLinks" ]);
}
