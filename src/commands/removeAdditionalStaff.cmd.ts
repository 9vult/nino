
import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";

export const RemoveAdditionalStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { commandName, options, user, member, guildId } = interaction;

  await interaction.deferReply();

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const episode = options.getNumber('episode')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let epvalue;
  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(`Guild ${guildId} does not exist.`, interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user!.id)
    return fail(`You do not have permission to do that.`, interaction);

  let success = false;
  for (let ep in projects[project].episodes)
    if (projects[project].episodes[ep].number == episode) {
      epvalue = ep;
      for (let pos in projects[project].episodes[ep].additionalStaff) {
        if (projects[project].episodes[ep].additionalStaff[pos].role.abbreviation == abbreviation) {
          success = true;
          db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}/additionalStaff`).child(pos).remove();
        }
      }
      if (success) {
        for (let task in projects[project].episodes[ep].tasks) {
          if (projects[project].episodes[ep].tasks[task].abbreviation == abbreviation)
            db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}/tasks`).child(task).remove();
        }
      }
    }
  if (!success)
    return fail(`Task ${abbreviation} was not found.`, interaction);

  const embed = new EmbedBuilder()
    .setTitle(`Project Modification`)
    .setDescription(`Removed position ${abbreviation} from episode ${episode}.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}