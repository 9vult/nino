import { ChatInputCommandInteraction, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "./generateAllowedMentions.action";

export const fail = async (faildesc: string, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const embed = new EmbedBuilder()
    .setTitle(`Baka.`)
    .setDescription(faildesc)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
  return false;
}