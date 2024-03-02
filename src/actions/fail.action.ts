import { CommandInteraction, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "./generateAllowedMentions.action";

export const fail = async (faildesc: string, interaction: CommandInteraction) => {
  if (!interaction.isCommand()) return;
  const embed = new EmbedBuilder()
    .setTitle(`:(`)
    .setDescription(faildesc)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
  return;
}