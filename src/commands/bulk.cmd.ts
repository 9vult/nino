import { ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const BulkCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const action = options.getString('action')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();
  const start_episode = options.getNumber('start_episode')!;
  const end_episode = options.getNumber('end_episode')!;

  let taskvalue;
  let taskName;
  let isValidUser = false;
  let episodeDone = true;
  let status = '';
  let header = '';

  let verification = await VerifyInteraction(dbdata, interaction, alias, false);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  if (start_episode > end_episode || start_episode == end_episode)
    return fail(t('invalidEpisodeRange', { lng }), interaction);

  for (let staff in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[staff];
    if (staffObj.role.abbreviation === abbreviation && (staffObj.id === user.id || projects[project].owner === user.id)) {
      isValidUser = true;
      taskName = staffObj.role.title;
      switch (action) {
        case 'Done':
          status = `✅ **${staffObj.role.title}**`;
          header = `✅ ${t('taskCompleteTitle', { lng })}`;
          break;
        case 'Undone':
          status = `❌ **${staffObj.role.title}**`;
          header = `❌ ${t('taskIncompleteTitle', { lng })}`;
          break;
        case 'Skip':
          status = `:fast_forward: **${staffObj.role.title}**`;
          header = `:fast_forward: ${t('taskSkippedTitle', { lng })}`;
          break;
      }
    }
  }

  const SET_VALUE = !(action === 'Undone');

  for (let ep in projects[project].episodes) {
    const epobj = projects[project].episodes[ep];
    if (epobj.number >= start_episode && epobj.number <= end_episode) {

      for (let task in epobj.tasks) {
        let taskObj = epobj.tasks[task];
        if (taskObj.abbreviation === abbreviation) {
          taskvalue = task;
          db.ref(`/Projects/${guildId}/${project}/episodes/${ep}/tasks/${taskvalue}`).update({
            abbreviation, done: SET_VALUE
          });
          const utc = Math.floor(new Date().getTime() / 1000);
          db.ref(`/Projects/${guildId}/${project}/episodes/${ep}`).update({
            updated: utc
          });
        }
        else if ((SET_VALUE && !taskObj.done) || !SET_VALUE) episodeDone = false;
      }

      if (taskvalue == undefined) return fail(t('noSuchTask', { lng, abbreviation }), interaction);
      
      db.ref(`/Projects/${guildId}/${project}/episodes/${ep}`).update({ done: episodeDone });
    }
  }

  if (!isValidUser)
    return fail(t('permissionDenied', { lng }), interaction);

  const embed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(header)
    .setDescription(t('bulkBody', { lng, taskName, start_episode, end_episode }))
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(`Episodes ${start_episode} — ${end_episode}`)
    .setThumbnail(projects[project].poster)
    .setDescription(status)
    .setTimestamp(Date.now());
  const publishChannel = client.channels.cache.get(projects[project].updateChannel);

  if (publishChannel?.isTextBased) {
    (publishChannel as TextChannel).send({ embeds: [publishEmbed] })
    .catch(err => console.error(`[Bulk]: "${err.message}" from guild ${guildId}, project ${projects[project].nickname}`));
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
            embeds: [ publishEmbed.toJSON() ]
          })
        });
      } catch {
        interaction.channel?.send(`Webhook ${observer.updatesWebhook} from ${observer.guildId} failed.`);
      }
    }
}