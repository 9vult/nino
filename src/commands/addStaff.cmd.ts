import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";

export const AddStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId } = interaction;

  await interaction.deferReply();

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const staff = (options.getMember('member')! as GuildMember).id;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();
  const title = options.getString('title')!;

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(`Guild ${guildId} does not exist.`, interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user!.id)
    return fail(`You do not have permission to do that.`, interaction);

  for (let pos in projects[project].keyStaff)
    if (projects[project].keyStaff[pos].role.abbreviation == abbreviation)
      return fail(`That position already exists.`, interaction);

  db.ref(`/Projects/${guildId}/${project}`).child("keyStaff").push({
    id: staff,
    role: {
      abbreviation,
      title
    }
  });

  const episodes = projects[project].episodes;
  for (let key in episodes) {
    db.ref(`/Projects/${guildId}/${project}/episodes/${key}`).child("tasks").push({
      abbreviation, done: false
    });
  }

  const embed = new EmbedBuilder()
    .setTitle(`Project Creation`)
    .setDescription(`Added <@${staff}> for position ${abbreviation}.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}
