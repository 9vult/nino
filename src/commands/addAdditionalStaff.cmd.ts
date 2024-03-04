
import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";

export const AddAdditionalStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { commandName, options, user, member, guildId } = interaction;

  await interaction.deferReply();

  const project = options.getString('project')!;
  const episode = options.getNumber('episode')!;
  const staff = (options.getMember('member')! as GuildMember).id;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();
  const title = options.getString('title')!;

  let epvalue;
  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(`Guild ${guildId} does not exist.`, interaction);

  let projects = dbdata.guilds[guildId];

  if (!(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user!.id)
    return fail(`You do not have permission to do that.`, interaction);

  for (let ep in projects[project].episodes)
    if (projects[project].episodes[ep].number == episode) {
      epvalue = ep;
      for (let pos in projects[project].episodes[ep].additionalStaff)
        if (projects[project].episodes[ep].additionalStaff[pos].role.abbreviation == abbreviation)
          return fail(`That position already exists.`, interaction);
    }

  db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}`).child("additionalStaff").push({
    id: staff,
    role: {
      abbreviation,
      title
    }
  });

  db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}`).child("tasks").push({
    abbreviation, done: false
  });

  const embed = new EmbedBuilder()
    .setTitle(`Project Creation`)
    .setDescription(`Added <@${staff}> for position ${abbreviation} for episode ${episode}.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}