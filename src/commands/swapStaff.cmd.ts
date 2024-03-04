import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";

export const SwapStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId } = interaction;

  await interaction.deferReply();

  const project = options.getString('project')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();
  const staff = (options.getMember('member')! as GuildMember).id;

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(`Guild ${guildId} does not exist.`, interaction);

  let projects = dbdata.guilds[guildId];

  if (!(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user!.id)
    return fail(`You do not have permission to do that.`, interaction);

  var found;
  for (let keystaff in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[keystaff];
    if (staffObj.role.abbreviation === abbreviation) {
      found = keystaff;
      db.ref(`/Projects/${guildId}/${project}`).child("keyStaff").child(found).update({ id: staff });
      break;
    }
  }

  if (found == undefined)
    return fail(`Position ${abbreviation} was not found.`, interaction);

  const embed = new EmbedBuilder()
    .setTitle(`Project Modification`)
    .setDescription(`Swapped <@${staff}> in for position ${abbreviation}.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}