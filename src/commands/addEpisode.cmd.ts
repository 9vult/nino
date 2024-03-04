import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project, Task } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";

export const AddEpisodeCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, member, guildId } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const project = options.getString('project')!;
  const number = options.getNumber('number')!;

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(`Guild ${guildId} does not exist.`, interaction);

  let projects = dbdata.guilds[guildId];

  if (!(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user!.id)
    return fail(`You do not have permission to do that.`, interaction);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);
  ref.update({ length: projects[project].length + 1 });
  
  let epref = ref.child('episodes').push({
    number: number,
    done: false,
    additionalStaff: [],
    tasks: []
  });
  
  for (let pos in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[pos]
    epref.child('tasks').push({
      abbreviation: staffObj.role.abbreviation,
      done: false
    });
  }

  const embed = new EmbedBuilder()
    .setTitle(`Project Modification`)
    .setDescription(`I added episode ${number} to \`${project}\` for you.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}