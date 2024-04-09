import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";

export const EditProjectCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, member, guildId } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const option = options.getString('option')!;
  const newValue = options.getString('newvalue')!;

  let projects = dbdata.guilds[guildId];
  if (!project || !(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user!.id)
    return fail(`You do not have permission to do that.`, interaction);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);

  switch (option) {
    case 'Title':
      ref.update({ title: newValue });
      break;
    case 'Poster':
      ref.update({ poster: newValue });
      break;
    case 'UpdateChannel':
      ref.update({ updateChannel: newValue });
      break;
    case 'ReleaseChannel':
      ref.update({ releaseChannel: newValue });
      break;
  }

  const embed = new EmbedBuilder()
    .setTitle(`Project Modification`)
    .setDescription(`I updated project \`${project}\` for you.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}