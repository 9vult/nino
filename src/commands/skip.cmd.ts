import { ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { EntriesToStatusString, GenerateEntries } from "../actions/generateEntries.action";
import { t } from "i18next";

export const SkipCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const selectedEpisode = options.getNumber('episode')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let epvalue;
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

  for (let keyStaffId in project.keyStaff) {
    let keyStaff = project.keyStaff[keyStaffId];
    if (keyStaff.role.abbreviation === abbreviation && (
      keyStaff.id === user.id || 
      project.owner === user.id || 
      project.administrators?.includes(user.id) ||
      dbdata.configuration[guildId!]?.administrators?.includes(user.id)
    )) {
      isValidUser = true;
      taskName = keyStaff.role.title;
      status = `:fast_forward: **${keyStaff.role.title}** ${t('skipped', { lng })}\n`;
    }
  }

  for (let epId in project.episodes) {
    const episode = project.episodes[epId];
    if (episode.number == selectedEpisode) {
      epvalue = epId;
      for (let taskId in episode.tasks) {
        let task = episode.tasks[taskId];
        if (task.abbreviation === abbreviation) {
          taskvalue = taskId;
          if (task.done)
            return fail(t('taskAlreadyDone', { lng, abbreviation }), interaction);
        }
        else if (!task.done) episodeDone = false;
        // Status string
        let stat;
        if (task.abbreviation === abbreviation) stat = `__~~${abbreviation}~~__ `;
        else if (task.done) stat = `~~${task.abbreviation}~~ `;
        else stat = `**${task.abbreviation}** `;

        localEntries[task.abbreviation].status = stat;
        status += EntriesToStatusString(localEntries);
      }
      if (taskvalue == undefined) return fail(t('noSuchTask', { lng, abbreviation }), interaction);
      if (!isValidUser) { // Not key staff
        for (let addStaffId in episode.additionalStaff) {
          let addStaff = episode.additionalStaff[addStaffId];
          if (addStaff.role.abbreviation === abbreviation && (
            addStaff.id === user.id ||
            project.owner === user.id ||
            project.administrators?.includes(user.id) ||
            dbdata.configuration[guildId!]?.administrators?.includes(user.id)
          )) {
            status = `:fast_forward: **${addStaff.role.title}** ${t('skipped', { lng })}\n` + status;
            taskName = addStaff.role.title;
            isValidUser = true;
          }
        }
      }
    }
  }

  if (!isValidUser)
    return fail(t('permissionDenied', { lng }), interaction);
  if (taskvalue != undefined) {
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${epvalue}/tasks/${taskvalue}`).update({
      abbreviation, done: true
    });
    const utc = Math.floor(new Date().getTime() / 1000);
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${epvalue!}`).update({
      updated: utc
    });
  }

  const episodeDoneText = episodeDone ? `\n${t('episodeDone', { lng, episode: selectedEpisode })}` : '';

  const succinctBody = `${t('taskSkippedBody', { lng, taskName, episode: selectedEpisode })}${episodeDoneText}`
  const verboseBody = `${t('taskSkippedBody', { lng, taskName, episode: selectedEpisode })}\n${EntriesToStatusString(localEntries)}${episodeDoneText}`;
  const succinctTitle = `:fast_forward: ${t('taskSkippedTitle', { lng })}`;
  const verboseTitle = `:fast_forward: Episode ${selectedEpisode}`;
  const useVerbose = dbdata.configuration[guildId!]?.doneDisplay === 'Verbose';

  const replyEmbed = new EmbedBuilder()
    .setAuthor({ name: `${project.title} (${project.type})` })
    .setTitle(useVerbose ? verboseTitle : succinctTitle)
    .setDescription(useVerbose ? verboseBody : succinctBody)
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });

  if (episodeDone) {
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${epvalue}`).update({ done: true });
  }

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: `${project.title} (${project.type})` })
    .setTitle(`Episode ${selectedEpisode}`)
    .setThumbnail(project.poster)
    .setDescription(status)
    .setTimestamp(Date.now());
  const publishChannel = client.channels.cache.get(project.updateChannel);

  if (publishChannel?.isTextBased) {
    (publishChannel as TextChannel).send({ embeds: [publishEmbed] })
    .catch(err => console.error(`[Skip]: "${err.message}" from guild ${guildId}, project ${project.nickname}`));
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
        interaction.channel?.send(`Webhook ${observer.updatesWebhook} from ${observer.guildId} failed.`);
      }
    }
}