import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project, Task } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";

export const RemoveEpisodeCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, member, guildId } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const number = options.getNumber('number')!;

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(`Guild ${guildId} does not exist.`, interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user!.id)
    return fail(`You do not have permission to do that.`, interaction);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);
  ref.update({ length: projects[project].length - 1 });

  for (let ep in projects[project].episodes) {
    if (projects[project].episodes[ep].number == number) {
      db.ref(`/Projects/${guildId}/${project}/episodes/${ep}`).remove();
      break;
    }
  }

  const embed = new EmbedBuilder()
    .setTitle(`Project Modification`)
    .setDescription(`I removed episode ${number} from \`${project}\` for you.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}