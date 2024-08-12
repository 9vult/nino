import { ChatInputCommandInteraction, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "./generateAllowedMentions.action";
import { nonce } from "./nonce";

export const info = async (faildesc: string, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const embed = new EmbedBuilder()
    .setTitle(`Info.`)
    .setDescription(faildesc)
    .setColor(0xd797ff);
  await interaction.channel?.send({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]), ...nonce() });
}