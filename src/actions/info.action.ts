import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "./generateAllowedMentions.action";
import { nonce } from "./nonce";
import { AlertError } from "./alertError";

export const info = async (faildesc: string, interaction: ChatInputCommandInteraction, client: Client) => {
  if (!interaction.isCommand()) return;
  const embed = new EmbedBuilder()
    .setTitle(`Info.`)
    .setDescription(faildesc)
    .setColor(0xd797ff);
  await interaction.followUp({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]), ...nonce() })

  // Fallback: Alert user of error and pass along message in DMs
  .catch(async (err) => {
    await AlertError(client, err, interaction.guildId ?? '-', `channel <#${interaction.channel?.id}>`, interaction.user.id, 'Info');
    await client.users.send(interaction.user.id, { content: '', embeds: [ embed ], ...nonce() }).catch();
  });
}