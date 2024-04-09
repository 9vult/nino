import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";

export const SwapAdditionalStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId } = interaction;

  await interaction.deferReply();

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const episode = options.getNumber('episode')!;
  const staff = (options.getMember('member')! as GuildMember).id;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(`Guild ${guildId} does not exist.`, interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user!.id)
    return fail(`You do not have permission to do that.`, interaction);

  var found;
  for (let ep in projects[project].episodes)
    if (projects[project].episodes[ep].number == episode) {
      for (let pos in projects[project].episodes[ep].additionalStaff)
        if (projects[project].episodes[ep].additionalStaff[pos].role.abbreviation == abbreviation) {
          found = pos;
          db.ref(`/Projects/${guildId}/${project}/episodes/${ep}`).child("additionalStaff").child(pos).update({ id: staff });
          break;
        }
    }

  if (found == undefined)
    return fail(`Position ${abbreviation} was not found.`, interaction);

  const embed = new EmbedBuilder()
    .setTitle(`Project Modification`)
    .setDescription(`Swapped <@${staff}> in for position ${abbreviation} for episode ${episode}.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}