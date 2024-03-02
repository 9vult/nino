
import { CacheType, Client, EmbedBuilder, Interaction } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { helpText } from "src/misc/misc";
import { VERSION } from "src/nino";

export const AboutCmd = async (interaction: Interaction<CacheType>) => {
  if (!interaction.isCommand()) return;
  const embed = new EmbedBuilder()
    .setTitle(`Nino Fansub Management Bot`)
    .setDescription(`Version: ${VERSION}\nAuthor: <@248600185423396866>`)
    .setURL(`https://github.com/9vult/nino`)
    .setColor(0xd797ff);
  await interaction.reply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}