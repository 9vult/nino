import { CacheType, Client, CommandInteraction, Embed, EmbedBuilder, Interaction, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import moment from "moment";

export const DoneCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: CommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId } = interaction;

  await interaction.deferReply();

  const project = String(options.get('project')!.value!);
  const episode = Number(options.get('episode')!.value!);
  const abbreviation = String(options.get('abbreviation')!.value!).toUpperCase();

  let epvalue;
  let taskvalue;
  let isValidUser = false;
  let status = '';
  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(`Guild ${guildId} does not exist.`, interaction);

  let projects = dbdata.guilds[guildId];

  if (!(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);

  if (projects[project].owner === user.id) isValidUser = true;
  for (let staff in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[staff];
    if (staffObj.id === user.id && staffObj.role.abbreviation === abbreviation) {
      isValidUser = true;
      status = `✅ **${staffObj.role.title}**\n`;
    }
  }

  for (let ep in projects[project].episodes) {
    if (projects[project].episodes[ep].number == episode) {
      epvalue = ep;
      for (let task in projects[project].episodes[ep].tasks) {
        let taskObj = projects[project].episodes[ep].tasks[task];
        if (taskObj.abbreviation === abbreviation) {
          taskvalue = task;
          if (taskObj.done)
            return fail(`Task ${abbreviation} is already done!`, interaction);
        }
        // Status string
        if (taskObj.abbreviation === abbreviation) status += `__~~${abbreviation}~~__ `;
        else if (taskObj.done) status += `~~${taskObj.abbreviation}~~ `;
        else status += `**${taskObj.abbreviation}** `;
      }
      if (taskvalue == undefined) return fail(`Task ${abbreviation} does not exist!`, interaction);
      if (!isValidUser) { // Not key staff
        for (let addStaff in projects[project].episodes[ep].additionalStaff) {
          let addStaffObj = projects[project].episodes[ep].additionalStaff[addStaff];
          if (addStaffObj.id === addStaffObj.id && addStaffObj.role.abbreviation === abbreviation) {
            status = `✅ **${addStaffObj.role.title}**\n` + status;
            isValidUser = true;
          }
        }
      }
    }
  }

  if (!isValidUser)
    return fail('You do not have permission to do that.', interaction);
  if (taskvalue != undefined)
    db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}/tasks/${taskvalue}`).update({
      abbreviation, done: true
    });

  const replyEmbed = new EmbedBuilder()
    .setAuthor({ name: projects[project].title })
    .setTitle(`Task Completed`)
    .setDescription(`Task ${abbreviation} has been completed. Nice job!`)
    .setColor(0xd797ff)
    .setFooter({ text: moment().format('MMMM D, YYYY h:mm:ss a') });
  await interaction.editReply({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: projects[project].title })
    .setTitle(`Episode ${episode}`)
    .setThumbnail(projects[project].poster)
    .setDescription(status)
    .setFooter({ text: moment().format('MMMM D, YYYY h:mm:ss a') });
  const publishChannel = client.channels.cache.get(projects[project].updateChannel);
  if (publishChannel?.isTextBased)
    (publishChannel as TextChannel).send({ embeds: [publishEmbed] })
}