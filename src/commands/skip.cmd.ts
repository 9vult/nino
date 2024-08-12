import { ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { EntriesToStatusString, GenerateEntries } from "../actions/generateEntries.action";
import { t } from "i18next";
import { getAdditionalStaff, getEpisode, getKeyStaff, getTask } from "../actions/getters";
import { AlertError } from "../actions/alertError";
import { nonce } from "../actions/nonce";

export const SkipCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const selectedEpisode = options.getNumber('episode')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let taskvalue;
  let taskName;
  let isValidUser = false;
  let status = '';
  let episodeDone = true;

  let verification = await VerifyInteraction(dbdata, interaction, alias, false);
  if (!verification) return;
  const { projects, project: projectName } = InteractionData(dbdata, interaction, alias);

  let localEntries = GenerateEntries(dbdata, guildId!, projectName, selectedEpisode);

  const project = projects[projectName];

  let { keyStaff } = getKeyStaff(project, abbreviation);

  if (keyStaff && (
    keyStaff.id === user.id ||
    project.owner === user.id ||
    project.administrators?.includes(user.id) ||
    dbdata.configuration[guildId!]?.administrators?.includes(user.id))
  ) {
    isValidUser = true;
    taskName = keyStaff.role.title;
    status = `:fast_forward: **${keyStaff.role.title}** ${t('progress.skipped.appendage', { lng })}\n`;
  }

  let { id: episodeId, episode } = getEpisode(project, selectedEpisode);
  if (!episode) return fail(t('error.noSuchEpisode', { lng, selectedEpisode }), interaction);

  for (let taskId in episode.tasks) {
    let task = episode.tasks[taskId];
    if (task.abbreviation === abbreviation) {
      taskvalue = taskId;
      if (task.done)
        return fail(t('error.progress.taskAlreadyDone', { lng, abbreviation }), interaction);
    }
    else if (!task.done) episodeDone = false;
    // Status string
    let stat;
    if (task.abbreviation === abbreviation) stat = `__~~${abbreviation}~~__ `;
    else if (task.done) stat = `~~${task.abbreviation}~~ `;
    else stat = `**${task.abbreviation}** `;

    localEntries[task.abbreviation].status = stat;
  }

  status += EntriesToStatusString(localEntries);
  if (taskvalue == undefined) return fail(t('error.noSuchTask', { lng, abbreviation }), interaction);

  if (!isValidUser) { // Not key staff
    let { addStaff } = getAdditionalStaff(episode, abbreviation);
    if (addStaff && (
      addStaff.id === user.id ||
      project.owner === user.id ||
      project.administrators?.includes(user.id) ||
      dbdata.configuration[guildId!]?.administrators?.includes(user.id)
    )) {
      status = `:fast_forward: **${addStaff.role.title}** ${t('progress.skipped.appendage', { lng })}\n` + status;
      taskName = addStaff.role.title;
      isValidUser = true;
    }
  }

  if (!isValidUser)
    return fail(t('error.permissionDenied', { lng }), interaction);
  
  if (taskvalue != undefined) {
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${episodeId}/tasks/${taskvalue}`).update({
      abbreviation, done: true
    });
    const utc = Math.floor(new Date().getTime() / 1000);
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${episodeId!}`).update({
      updated: utc
    });
  }

  const episodeDoneText = episodeDone ? `\n${t('progress.episodeComplete', { lng, episode: selectedEpisode })}` : '';

  const succinctBody = `${t('progress.skipped', { lng, taskName, episode: selectedEpisode })}${episodeDoneText}`
  const verboseBody = `${t('progress.skipped', { lng, taskName, episode: selectedEpisode })}\n\n${EntriesToStatusString(localEntries)}${episodeDoneText}`;
  const useVerbose = dbdata.configuration[guildId!]?.doneDisplay === 'Verbose';

  const lock = project.isPrivate ? '🔒 ' : '';

  const replyEmbed = new EmbedBuilder()
    .setAuthor({ name: `${lock}${project.title} (${project.type})` })
    .setTitle(`:fast_forward: ${t('title.taskSkipped', { lng })}`)
    .setDescription(useVerbose ? verboseBody : succinctBody)
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });

  if (episodeDone) {
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${episodeId}`).update({ done: true });
  }

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: `${project.title} (${project.type})` })
    .setTitle(`Episode ${selectedEpisode}`)
    .setThumbnail(project.poster)
    .setDescription(status)
    .setTimestamp(Date.now());
  const publishChannel = client.channels.cache.get(project.updateChannel);

  if (publishChannel?.isTextBased) {
    (publishChannel as TextChannel).send({ embeds: [publishEmbed], ...nonce() })
    .catch(err => AlertError(client, err, guildId!, project.nickname, 'Skip'));
  }

  if (!project.observers) return; // Stop here if there's no observers
  for (let observerid in project.observers) {
    const observer = project.observers[observerid];
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
      interaction.channel?.send({
        content: `Webhook ${observer.updatesWebhook} from ${observer.guildId} failed.`,
        ...nonce()
      });
    }
  }
}