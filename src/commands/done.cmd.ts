import { ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { EntriesToStatusString, GenerateEntries } from "../actions/generateEntries.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";

export const DoneCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId } = interaction;

  await interaction.deferReply();
  const locale = interaction.locale;

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const episode = options.getNumber('episode')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let epvalue;
  let taskvalue;
  let taskName;
  let isValidUser = false;
  let episodeDone = true;
  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(interp(GetStr(dbdata.i18n, 'noSuchGuild', locale), { '$GUILDID': guildId }), interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(interp(GetStr(dbdata.i18n, 'noSuchproject', interaction.locale), { '$PROJECT': project }), interaction);

  let status = '';
  let entries = GenerateEntries(dbdata, guildId, project, episode);

  for (let staff in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[staff];
    if (staffObj.role.abbreviation === abbreviation && (staffObj.id === user.id || projects[project].owner === user.id)) {
      isValidUser = true;
      taskName = staffObj.role.title;
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
            return fail(interp(GetStr(dbdata.i18n, 'taskAlreadyDone', interaction.locale), { '$ABBREVIATION': abbreviation }), interaction);
        }
        else if (!taskObj.done) episodeDone = false;
        // Status string
        if (taskObj.abbreviation === abbreviation) entries[taskObj.abbreviation].status = `__~~${abbreviation}~~__`;
        else if (taskObj.done) entries[taskObj.abbreviation].status = `~~${taskObj.abbreviation}~~`;
        else entries[taskObj.abbreviation].status = `**${taskObj.abbreviation}**`;
      }

      status += EntriesToStatusString(entries);

      if (taskvalue == undefined) return fail(interp(GetStr(dbdata.i18n, 'noSuchTask', interaction.locale), { '$ABBREVIATION': abbreviation }), interaction);
      if (!isValidUser) { // Not key staff
        for (let addStaff in projects[project].episodes[ep].additionalStaff) {
          let addStaffObj = projects[project].episodes[ep].additionalStaff[addStaff];
          if (addStaffObj.role.abbreviation === abbreviation && (addStaffObj.id === user.id || projects[project].owner === user.id)) {
            status = `✅ **${addStaffObj.role.title}**\n` + status;
            taskName = addStaffObj.role.title;
            isValidUser = true;
          }
        }
      }
    }
  }

  if (!isValidUser)
    return fail(GetStr(dbdata.i18n, 'permissionDenied', locale), interaction);
  if (taskvalue != undefined)
    db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}/tasks/${taskvalue}`).update({
      abbreviation, done: true
    });

  const episodeDoneText = episodeDone ? `\n${interp(GetStr(dbdata.i18n, 'episodeDone', interaction.locale), { '$EPISODE': episode })}` : '';
  const replyEmbed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(`✅ ${GetStr(dbdata.i18n, 'taskCompleteTitle', interaction.locale)}`)
    .setDescription(`${interp(GetStr(dbdata.i18n, 'taskCompleteBody', interaction.locale), { '$TASKNAME': taskName, '$EPISODE': episode })}${episodeDoneText}`)
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });

  if (episodeDone) {
    db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}`).update({ done: true });
  }

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(`Episode ${episode}`)
    .setThumbnail(projects[project].poster)
    .setDescription(status)
    .setTimestamp(Date.now());
  const publishChannel = client.channels.cache.get(projects[project].updateChannel);
  if (publishChannel?.isTextBased)
    (publishChannel as TextChannel).send({ embeds: [publishEmbed] })

  if (!projects[project].observers) return; // Stop here if there's no observers
  for (let observerid in projects[project].observers) {
    const observer = projects[project].observers[observerid];
    if (!observer.updatesWebhook) continue;
    try {
      fetch(observer.updatesWebhook, {
        method: 'POST',
        headers: { 'content-type': 'application/json' },
        body: JSON.stringify({
          username: 'Nino',
          avatar_url: 'https://i.imgur.com/PWtteaY.png',
          content: '',
          embeds: [ publishEmbed.toJSON() ]
        })
      });
    } catch {
      interaction.channel?.send(`Webhook ${observer.updatesWebhook} failed.`);
    }
  }
}