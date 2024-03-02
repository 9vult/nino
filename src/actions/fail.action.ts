import { CacheType, EmbedBuilder, Interaction } from "discord.js";
import { generateAllowedMentions } from "./generateAllowedMentions.action";

export const fail = async (faildesc: string, interaction: Interaction<CacheType>) => {
  if (!interaction.isCommand()) return;
  if (faildesc !== undefined) {
    const embed = new EmbedBuilder()
      .setTitle(`Project Creation`)
      .setDescription(faildesc)
      .setColor(0xd797ff);
    await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
    return;
  }
}