import { ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const SkipCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const episode = options.getNumber('episode')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let epvalue;
  let taskvalue;
  let taskName;
  let isValidUser = false;
  let status = '';
  let episodeDone = true;

  let verification = await VerifyInteraction(dbdata, interaction, alias, false);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  for (let staff in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[staff];
    if (staffObj.role.abbreviation === abbreviation && (staffObj.id === user.id || projects[project].owner === user.id)) {
      isValidUser = true;
      taskName = staffObj.role.title;
      status = `:fast_forward: **${staffObj.role.title}** ${t('skipped', { lng })}\n`;
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
            return fail(t('taskAlreadyDone', { lng, abbreviation }), interaction);
        }
        else if (!taskObj.done) episodeDone = false;
        // Status string
        if (taskObj.abbreviation === abbreviation) status += `__~~${abbreviation}~~__ `;
        else if (taskObj.done) status += `~~${taskObj.abbreviation}~~ `;
        else status += `**${taskObj.abbreviation}** `;
      }
      if (taskvalue == undefined) return fail(t('noSuchTask', { lng, abbreviation }), interaction);
      if (!isValidUser) { // Not key staff
        for (let addStaff in projects[project].episodes[ep].additionalStaff) {
          let addStaffObj = projects[project].episodes[ep].additionalStaff[addStaff];
          if (addStaffObj.role.abbreviation === abbreviation && (addStaffObj.id === user.id || projects[project].owner === user.id)) {
            status = `:fast_forward: **${addStaffObj.role.title}** ${t('skipped', { lng })}\n` + status;
            taskName = addStaffObj.role.title;
            isValidUser = true;
          }
        }
      }
    }
  }

  if (!isValidUser)
    return fail(t('permissionDenied', { lng }), interaction);
  if (taskvalue != undefined) {
    db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}/tasks/${taskvalue}`).update({
      abbreviation, done: true
    });
    const utc = Math.floor(new Date().getTime() / 1000);
    db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue!}`).update({
      updated: utc
    });
  }

  const episodeDoneText = episodeDone ? `\n${t('episodeDone', { lng, episode })}` : '';
  const replyEmbed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(`:fast_forward: ${t('taskSkippedTitle', { lng })}`)
    .setDescription(`${t('taskSkippedBody', { lng, taskName, episode })}${episodeDoneText}`)
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

  if (publishChannel?.isTextBased) {
    (publishChannel as TextChannel).send({ embeds: [publishEmbed] })
    .catch(err => console.error(err));
  }

  if (!projects[project].observers) return; // Stop here if there's no observers
    for (let observerid in projects[project].observers) {
      const observer = projects[project].observers[observerid];
      if (!observer.updatesWebhook) continue;
      try {
        const postUrl = new URL(observer.updatesWebhook);
          fetch(postUrl, {
          method: 'POST',
          headers: { 'content-type': 'application/json' },
          body: JSON.stringify({
            username: 'Nino',
            avatar_url: 'https://i.imgur.com/PWtteaY.png',
            content: '',
            embeds: [publishEmbed.toJSON()]
          })
        });
      } catch {
        interaction.channel?.send(`Webhook ${observer.updatesWebhook} from ${observer.guildId} failed.`);
      }
    }
}