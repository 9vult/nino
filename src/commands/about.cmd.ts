
import { ChatInputCommandInteraction, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { VERSION } from "../nino";

export const AboutCmd = async (interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const embed = new EmbedBuilder()
    .setTitle(`Nino Fansub Management Bot`)
    .setDescription(`Version: ${VERSION}\nAuthor: <@248600185423396866>`)
    .setURL(`https://github.com/9vult/nino`)
    .setColor(0xd797ff);
  await interaction.reply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}