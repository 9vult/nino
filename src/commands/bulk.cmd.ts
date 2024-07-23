import { ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";
import { getKeyStaff } from "../actions/getters";

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
  const { projects, project: projectName } = InteractionData(dbdata, interaction, alias);

  if (start_episode > end_episode || start_episode == end_episode)
    return fail(t('error.invalidTimeRange', { lng }), interaction);

  const project = projects[projectName];

  let { keyStaff } = getKeyStaff(project, abbreviation);
  if (!keyStaff) return fail(t('error.noSuchTask', { lng, abbreviation }), interaction);

  if (keyStaff.id === user.id || project.owner === user.id) {
    isValidUser = true;
    taskName = keyStaff.role.title;
    switch (action) {
      case 'Done':
        status = `✅ **${keyStaff.role.title}**`;
        header = `✅ ${t('title.taskComplete', { lng })}`;
        break;
      case 'Undone':
        status = `❌ **${keyStaff.role.title}**`;
        header = `❌ ${t('title.taskIncomplete', { lng })}`;
        break;
      case 'Skip':
        status = `:fast_forward: **${keyStaff.role.title}**`;
        header = `:fast_forward: ${t('title.taskSkipped', { lng })}`;
        break;
    }
  }

  const SET_VALUE = !(action === 'Undone');

  for (let epId in project.episodes) {
    const episode = project.episodes[epId];
    if (episode.number >= start_episode && episode.number <= end_episode) {

      for (let taskId in episode.tasks) {
        let task = episode.tasks[taskId];
        if (task.abbreviation === abbreviation) {
          taskvalue = taskId;
          db.ref(`/Projects/${guildId}/${projectName}/episodes/${epId}/tasks/${taskvalue}`).update({
            abbreviation, done: SET_VALUE
          });
          const utc = Math.floor(new Date().getTime() / 1000);
          db.ref(`/Projects/${guildId}/${projectName}/episodes/${epId}`).update({
            updated: utc
          });
        }
        else if ((SET_VALUE && !task.done) || !SET_VALUE) episodeDone = false;
      }

      if (taskvalue == undefined) return fail(t('error.noSuchTask', { lng, abbreviation }), interaction);
      
      db.ref(`/Projects/${guildId}/${projectName}/episodes/${epId}`).update({ done: episodeDone });
    }
  }

  if (!isValidUser)
    return fail(t('error.permissionDenied', { lng }), interaction);

  const embed = new EmbedBuilder()
    .setAuthor({ name: `${project.title} (${project.type})` })
    .setTitle(header)
    .setDescription(t('progress.bulk', { lng, taskName, start_episode, end_episode }))
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: `${project.title} (${project.type})` })
    .setTitle(`Episodes ${start_episode} — ${end_episode}`)
    .setThumbnail(project.poster)
    .setDescription(status)
    .setTimestamp(Date.now());
  const publishChannel = client.channels.cache.get(project.updateChannel);

  if (publishChannel?.isTextBased) {
    (publishChannel as TextChannel).send({ embeds: [publishEmbed] })
    .catch(err => console.error(`[Bulk]: "${err.message}" from guild ${guildId}, project ${project.nickname}`));
  }

  if (!project.observers) return; // Stop here if there's no observers
    for (let observerId in project.observers) {
      const observer = project.observers[observerId];
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