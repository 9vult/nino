
import { CacheType, Client, EmbedBuilder, Interaction } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { helpText } from "src/misc/misc";

export const HelpCmd = async (interaction: Interaction<CacheType>) => {
  if (!interaction.isCommand()) return;
  const embed = new EmbedBuilder()
    .setTitle(`Nino Help`)
    .setDescription(helpText)
    .setColor(0xd797ff);

  await interaction.reply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}